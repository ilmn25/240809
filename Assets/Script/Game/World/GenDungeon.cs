using System.Collections.Generic;
using UnityEngine;

public class GenDungeon : Gen
{
    private const int CeilingY = World.ChunkSize - 1;   // top brick layer; interior is y=1..CeilingY-1
    private const int MaxRooms = 70;

    // DungeonRoomLoot side rooms scattered through the dungeon.
    private const int LootRoomsPerDungeon = 4;
    private const int LootRoomClearance = 48;    // preferred gap (tiles) from the spawn and other loot rooms
    private const int LootRoomMinClearance = 16; // relaxed gap when the layout can't fit enough spread-out rooms

    // Scatter placed on floor tiles; thresholds are cumulative, checked in order.
    private static readonly (float Chance, ID Id)[] Scatter =
    {
        (0.012f, ID.Rubble),
        (0.006f, ID.OilBarrel),
        (0.010f, ID.OldPot),
        (0.006f, ID.Skeleton),
    };

    private static readonly Chunk Exit = SetPiece.LoadSetPieceFile("DungeonExit");
    private static readonly Chunk Boss = SetPiece.LoadSetPieceFile("DungeonRoomBoss");
    private static readonly Chunk LootRoom = SetPiece.LoadSetPieceFile("DungeonRoomLoot");
    // Where the exit piece's doorway sits at the spawn point (piece-local x, z).
    private static readonly Vector2Int ExitDoor = new Vector2Int(6, 11);

    private static int _stoneId, _brickId;
    private static int Stone => _stoneId == 0 ? Block.ConvertID(ID.StoneBlock) : _stoneId;
    private static int Brick => _brickId == 0 ? Block.ConvertID(ID.BrickBlock) : _brickId;

    public override Vector3Int GetSize() => new Vector3Int(20, 1, 20);

    public override Vector3Int GetSpawnPoint()
    {
        int c = GetSize().x / 2 * World.ChunkSize;
        return new Vector3Int(c, 2, c);
    }

    protected override void GenChunk(Vector3Int currentCoordinate, Chunk currentChunk)
    {
        if (currentCoordinate.y != 0) return;

        // Solid stone matrix that the rooms are carved into.
        int cs = World.ChunkSize;
        for (int y = 0; y < cs; y++)
            for (int x = 0; x < cs; x++)
                for (int z = 0; z < cs; z++)
                    currentChunk[x, y, z] = Stone;
    }

    protected override void GenPostWorld(World world)
    {
        int width = world.Size.x * World.ChunkSize;
        int depth = world.Size.z * World.ChunkSize;
        int seed = (int)GetDeterministicOffset("DungeonLayout");

        DungeonTile[,] grid = DungeonLayout.Generate(width, depth, MaxRooms, seed);
        RenderGrid(world, grid, width, depth, seed);

        Vector2Int spawnXZ = new Vector2Int(GetSpawnPoint().x, GetSpawnPoint().z);

        // Exit stairwell: paste so its doorway (piece-local ExitDoor) sits at the
        // dungeon spawn point.
        Vector2Int exitOrigin = Exit != null ? spawnXZ - ExitDoor : Vector2Int.zero;
        Paste(world, Exit, exitOrigin);

        // Boss room at the floor tile farthest from the exit.
        Vector2Int bossCentre = FarthestFloor(grid, width, depth, spawnXZ);
        Vector2Int bossOrigin = PieceOrigin(bossCentre, Boss.size, width, depth);
        Paste(world, Boss, bossOrigin);

        // Scatter DungeonRoomLoot side rooms across all-Floor squares so each one
        // keeps RenderGrid's full-height carve (no low ceiling) and never lands
        // on the boss or exit rooms.
        RectInt bossRect = new RectInt(bossOrigin.x, bossOrigin.y, Boss.size, Boss.size);
        RectInt exitRect = Exit == null ? new RectInt()
                                        : new RectInt(exitOrigin.x, exitOrigin.y, Exit.size, Exit.size);
        List<Vector2Int> lootCentres = FindLootSpots(grid, bossRect, exitRect, spawnXZ,
            LootRoom.size, LootRoomsPerDungeon, LootRoomClearance, seed);
        foreach (Vector2Int centre in lootCentres)
            Paste(world, LootRoom, PieceOrigin(centre, LootRoom.size, width, depth));
    }

    private static Vector2Int PieceOrigin(Vector2Int centre, int size, int width, int depth)
        => new Vector2Int(Mathf.Clamp(centre.x - size / 2, 0, width - size),
                          Mathf.Clamp(centre.y - size / 2, 0, depth - size));

    /// <summary>Pastes a set piece on the y=0 plane at <paramref name="originXZ"/>.</summary>
    private static void Paste(World world, Chunk setPiece, Vector2Int originXZ)
    {
        if (setPiece == null) return;
        SetPiece.Paste(world, new Vector3Int(originXZ.x, 0, originXZ.y), setPiece);
    }
 
    private static List<Vector2Int> FindLootSpots(DungeonTile[,] grid, RectInt bossRect,
        RectInt exitRect, Vector2Int spawn, int roomSize, int count, int clearance, int seed)
    {
        List<Vector2Int> centres = AllFloorCentres(grid, bossRect, exitRect, roomSize);

        // Shuffle so the greedy pass doesn't always favour the same region.
        System.Random rng = new System.Random(seed ^ 0x10AD);
        for (int i = centres.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (centres[i], centres[j]) = (centres[j], centres[i]);
        }

        // Never butt a loot room against the boss room wall.
        Vector2Int bossCentre = new Vector2Int(bossRect.x + bossRect.width / 2,
                                               bossRect.y + bossRect.height / 2);
        long bossMinDist2 = Sqr((bossRect.width + roomSize) / 2 + 2);

        List<Vector2Int> chosen = new List<Vector2Int>();
        foreach (int gap in new[] { clearance, clearance / 2, LootRoomMinClearance })
        {
            if (chosen.Count == count) break;
            long gap2 = Sqr(gap);
            foreach (Vector2Int centre in centres)
            {
                if (chosen.Count == count) break;
                if (TooClose(centre, spawn, gap2) || TooClose(centre, bossCentre, bossMinDist2)) continue;
                if (chosen.Exists(p => TooClose(centre, p, gap2))) continue;
                chosen.Add(centre);
            }
        }
        return chosen;
    }

    /// <summary>The centre of every <paramref name="size"/>×<paramref name="size"/>
    /// all-Floor square that doesn't overlap the boss or exit footprints.</summary>
    private static List<Vector2Int> AllFloorCentres(DungeonTile[,] grid, RectInt bossRect,
        RectInt exitRect, int size)
    {
        int width = grid.GetLength(0), depth = grid.GetLength(1);
        int half = size / 2;
        List<Vector2Int> centres = new List<Vector2Int>();
        for (int x = 0; x + size <= width; x++)
            for (int z = 0; z + size <= depth; z++)
            {
                if (Intersects(x, z, size, bossRect) || Intersects(x, z, size, exitRect)) continue;
                if (!IsAllFloor(grid, x, z, size)) continue;
                centres.Add(new Vector2Int(x + half, z + half));
            }
        return centres;
    }

    private static bool IsAllFloor(DungeonTile[,] grid, int x, int z, int size)
    {
        for (int dx = 0; dx < size; dx++)
            for (int dz = 0; dz < size; dz++)
                if (grid[x + dx, z + dz] != DungeonTile.Floor) return false;
        return true;
    }

    /// <summary>Whether a <paramref name="size"/>-square with top-left corner
    /// (<paramref name="x"/>, <paramref name="z"/>) overlaps <paramref name="rect"/>.</summary>
    private static bool Intersects(int x, int z, int size, RectInt rect)
        => rect.width > 0 &&
           x < rect.xMax && x + size > rect.x &&
           z < rect.yMax && z + size > rect.y;

    private static bool TooClose(Vector2Int a, Vector2Int b, long minDist2) => SqrDist(a, b) < minDist2;

    private static long SqrDist(Vector2Int a, Vector2Int b)
    {
        long dx = a.x - b.x, dz = a.y - b.y;
        return dx * dx + dz * dz;
    }

    private static long Sqr(long v) => v * v;

    /// <summary>The floor tile furthest (squared distance) from <paramref name="from"/>.</summary>
    private static Vector2Int FarthestFloor(DungeonTile[,] grid, int width, int depth, Vector2Int from)
    {
        Vector2Int best = from;
        long bestDist = -1;
        for (int x = 0; x < width; x++)
            for (int z = 0; z < depth; z++)
            {
                if (grid[x, z] != DungeonTile.Floor) continue;
                long dx = x - from.x, dz = z - from.y;
                long dist = dx * dx + dz * dz;
                if (dist > bestDist) { bestDist = dist; best = new Vector2Int(x, z); }
            }
        return best;
    }

    private static void RenderGrid(World world, DungeonTile[,] grid, int width, int depth, int seed)
    {
        System.Random rng = new System.Random(seed ^ 0x5EED);
        // Carve each floor column (brick floor/ceiling, air interior), then scatter.
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                if (grid[x, z] != DungeonTile.Floor) continue;

                GenBlocks.SetBlock(world, x, 0, z, Brick);
                for (int y = 1; y < CeilingY; y++)
                    GenBlocks.SetBlock(world, x, y, z, 0);
                GenBlocks.SetBlock(world, x, CeilingY, z, Brick);
                for (int y = CeilingY + 1; y < World.ChunkSize; y++)
                    GenBlocks.SetBlock(world, x, y, z, 0);

                double roll = rng.NextDouble();
                float acc = 0f;
                foreach ((float chance, ID id) in Scatter)
                {
                    acc += chance;
                    if (roll < acc) { GenBlocks.PlaceEntity(world, new Vector3Int(x, 1, z), id); break; }
                }
            }
        }
    }
}
