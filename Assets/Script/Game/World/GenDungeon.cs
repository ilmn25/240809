using System.Collections.Generic;
using UnityEngine;

public class GenDungeon : GenSocketRooms
{
    // DungeonLoot side rooms scattered through the dungeon.
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

    // The room pieces the layout fills the dungeon with.
    private static readonly Chunk[] Pieces =
    {
        SetPiece.LoadSetPieceFile("Dungeon"),
        SetPiece.LoadSetPieceFile("DungeonCross"),
        SetPiece.LoadSetPieceFile("DungeonPillars"),
        SetPiece.LoadSetPieceFile("DungeonCave"),
        SetPiece.LoadSetPieceFile("DungeonCells"),
        SetPiece.LoadSetPieceFile("DungeonTreasure"),
    };

    private static readonly Chunk Exit = SetPiece.LoadSetPieceFile("DungeonExit");
    private static readonly Chunk Boss = SetPiece.LoadSetPieceFile("DungeonBoss");
    private static readonly Chunk LootRoom = SetPiece.LoadSetPieceFile("DungeonLoot");
    // Where the exit piece's doorway sits at the spawn point (piece-local x, z).
    private static readonly Vector2Int ExitDoor = new Vector2Int(6, 11);

    private static int _stoneId;
    private static int Stone => _stoneId == 0 ? Block.ConvertID(ID.StoneBlock) : _stoneId;

    protected override int MatrixBlock => Stone;
    protected override string LayoutSalt => "DungeonLayout";
    protected override Chunk[] RoomPieces => Pieces;

    protected override void GenPostWorld(World world)
    {
        // The rooms are authored set pieces; the layout decides where each one goes.
        BuildLayout(world);
        int width = LayoutWidth, depth = LayoutDepth;
        ScatterFloor(world, Plan.grid, width, depth, LayoutSeed);

        // The spawn sits on the anchor room's centre (piece-local ExitDoor lands there).
        Vector3Int spawn = GetSpawnPoint();
        Vector2Int spawnXZ = new Vector2Int(spawn.x, spawn.z);

        // Exit stairwell: paste so its doorway (piece-local ExitDoor) sits at the
        // dungeon spawn point.
        Vector2Int exitOrigin = Exit != null ? spawnXZ - ExitDoor : Vector2Int.zero;
        Paste(world, Exit, exitOrigin);

        // Boss room at the floor tile farthest from the exit.
        Vector2Int bossCentre = FarthestFloor(Plan.grid, width, depth, spawnXZ);
        Vector2Int bossOrigin = PieceOrigin(bossCentre, Boss.size, width, depth);
        Paste(world, Boss, bossOrigin);

        // Scatter DungeonLoot side rooms across all-Floor squares so each one never
        // lands on the boss or exit rooms.
        RectInt bossRect = new RectInt(bossOrigin.x, bossOrigin.y, Boss.size, Boss.size);
        RectInt exitRect = Exit == null ? new RectInt()
                                        : new RectInt(exitOrigin.x, exitOrigin.y, Exit.size, Exit.size);
        List<Vector2Int> lootCentres = FindLootSpots(Plan.grid, bossRect, exitRect, spawnXZ,
            LootRoom.size, LootRoomsPerDungeon, LootRoomClearance, LayoutSeed);
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

    /// <summary>Drops the floor clutter on every walkable tile of the layout.</summary>
    private static void ScatterFloor(World world, DungeonTile[,] grid, int width, int depth, int seed)
    {
        System.Random rng = new System.Random(seed ^ 0x5EED);
        for (int x = 0; x < width; x++)
            for (int z = 0; z < depth; z++)
            {
                if (grid[x, z] != DungeonTile.Floor) continue;

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
