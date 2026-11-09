using System.Collections.Generic;
using UnityEngine;

public class GenDungeon : GenSocketRooms
{
    // A floor is small on purpose: a handful of rooms to fight through, not a maze.
    private const int ChunksXZ = 12;
    private const int Rooms = 30;

    // DungeonLoot side rooms scattered through the floor; deeper floors hold more.
    private const int BaseLootRooms = 2;
    private const int MaxLootRooms = 6;
    private const int LootRoomClearance = 24;    // preferred gap (tiles) from the spawn and other loot rooms
    private const int LootRoomMinClearance = 8;  // relaxed gap when the layout can't fit enough spread-out rooms

    /// <summary>Minimum gap (blocks) between the spawn stairwell and the stairwell down,
    /// so a floor is always a trek and the two doors can never be mistaken for each other.</summary>
    private const int MinStairwellGap = 60;

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

    /// <summary>The brick stairwell piece, used both for the way back up and the way down.</summary>
    private static readonly Chunk Stairwell = SetPiece.LoadSetPieceFile("DungeonExit");
    private static readonly Chunk Boss = SetPiece.LoadSetPieceFile("DungeonBoss");
    private static readonly Chunk LootRoom = SetPiece.LoadSetPieceFile("DungeonLoot");
    // Where the stairwell's doorway sits at the spawn point (piece-local x, z).
    private static readonly Vector2Int StairwellDoor = new Vector2Int(6, 11);

    private static int _stoneId;
    private static int Stone => _stoneId == 0 ? Block.ConvertID(ID.StoneBlock) : _stoneId;

    /// <summary>Floor of the current run: 1 is the first floor below the surface.</summary>
    private static int Depth => Save.Inst.DungeonDepth;

    protected override int MatrixBlock => Stone;
    protected override string LayoutSalt => "DungeonLayout";
    protected override Chunk[] RoomPieces => Pieces;

    /// <summary>Each visit rebuilds the floor from scratch, so every dive is a new run.</summary>
    public override bool RegenerateOnVisit => true;

    public override Vector3Int GetSize() => new Vector3Int(ChunksXZ, 1, ChunksXZ);
    protected override int MaxRooms => Rooms;

    protected override void GenPostWorld(World world)
    {
        // The rooms are authored set pieces; the layout decides where each one goes.
        BuildLayout(world);
        int width = LayoutWidth, depth = LayoutDepth;
        ScatterFloor(world, Plan.grid, width, depth, LayoutSeed);

        // The player arrives on the spawn point — the doorway of the way back up.
        Vector3Int spawn = GetSpawnPoint();
        Vector2Int spawnXZ = new Vector2Int(spawn.x, spawn.z);

        RectInt upRect = new RectInt(), downRect = new RectInt();
        Vector2Int downXZ = new Vector2Int(-1, -1);
        if (Stairwell != null)
        {
            PasteStairwell(world, spawnXZ);
            upRect = StairwellRect(spawnXZ);

            // The way down: the deepest tile that can hold the stairwell piece, far enough
            // from the spawn that every floor is a trek across it. Preferred tile is one the
            // door can be reached from; the fallbacks only matter on very small floors.
            downXZ = FarthestFittingFloor(Plan.grid, width, depth, spawnXZ,
                Stairwell.size, StairwellDoor, MinStairwellGap, openDoorway: true);
            if (downXZ.x < 0)
                downXZ = FarthestFittingFloor(Plan.grid, width, depth, spawnXZ,
                    Stairwell.size, StairwellDoor, MinStairwellGap, openDoorway: false);
            if (downXZ.x < 0)
                downXZ = FarthestFittingFloor(Plan.grid, width, depth, spawnXZ,
                    Stairwell.size, StairwellDoor, 0, openDoorway: false);
            if (downXZ.x >= 0)
            {
                PasteStairwell(world, downXZ);
                downRect = StairwellRect(downXZ);
            }
        }

        // Boss vault on the tile farthest from the descent — you pass it on the way down,
        // not on the way in — and clear of both stairwells.
        Vector2Int bossCentre = FarthestFloor(Plan.grid, width, depth,
            downXZ.x >= 0 ? downXZ : spawnXZ, upRect, downRect);
        Vector2Int bossOrigin = PieceOrigin(bossCentre, Boss.size, width, depth);
        Paste(world, Boss, bossOrigin);

        // Loot vaults across all-Floor squares, kept off the boss, spawn and descend rooms.
        RectInt bossRect = new RectInt(bossOrigin.x, bossOrigin.y, Boss.size, Boss.size);
        int lootRooms = Mathf.Min(MaxLootRooms, BaseLootRooms + (Depth - 1) / 2);
        List<Vector2Int> lootCentres = FindLootSpots(Plan.grid, bossRect, spawnXZ,
            LootRoom.size, lootRooms, LootRoomClearance, LayoutSeed, upRect, downRect);
        foreach (Vector2Int centre in lootCentres)
            Paste(world, LootRoom, PieceOrigin(centre, LootRoom.size, width, depth));
    }

    /// <summary>Pastes the stairwell so its doorway (<see cref="StairwellDoor"/> in piece
    /// space) lands on <paramref name="doorway"/>.</summary>
    private static void PasteStairwell(World world, Vector2Int doorway)
        => Paste(world, Stairwell, doorway - StairwellDoor);

    /// <summary>Footprint of the stairwell whose doorway sits on <paramref name="doorway"/>.</summary>
    private static RectInt StairwellRect(Vector2Int doorway)
        => new RectInt(doorway.x - StairwellDoor.x, doorway.y - StairwellDoor.y,
                       Stairwell.size, Stairwell.size);

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
        Vector2Int spawn, int roomSize, int count, int clearance, int seed, params RectInt[] blocked)
    {
        List<Vector2Int> centres = AllFloorCentres(grid, bossRect, roomSize, blocked);

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

    /// <summary>All-Floor square centres of <paramref name="size"/> that don't overlap
    /// the boss footprint or any <paramref name="blocked"/> rect (spawn/descend rooms).</summary>
    private static List<Vector2Int> AllFloorCentres(DungeonTile[,] grid, RectInt bossRect,
        int size, RectInt[] blocked)
    {
        int width = grid.GetLength(0), depth = grid.GetLength(1);
        int half = size / 2;
        List<Vector2Int> centres = new List<Vector2Int>();
        for (int x = 0; x + size <= width; x++)
            for (int z = 0; z + size <= depth; z++)
            {
                if (Intersects(x, z, size, bossRect) || IntersectsAny(x, z, size, blocked)) continue;
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

    /// <summary>Whether a <paramref name="size"/>-square at (<paramref name="x"/>,
    /// <paramref name="z"/>) overlaps any of <paramref name="rects"/>.</summary>
    private static bool IntersectsAny(int x, int z, int size, RectInt[] rects)
    {
        foreach (RectInt rect in rects)
            if (Intersects(x, z, size, rect)) return true;
        return false;
    }

    private static bool TooClose(Vector2Int a, Vector2Int b, long minDist2) => SqrDist(a, b) < minDist2;

    private static long SqrDist(Vector2Int a, Vector2Int b)
    {
        long dx = a.x - b.x, dz = a.y - b.y;
        return dx * dx + dz * dz;
    }

    private static long Sqr(long v) => v * v;

    /// <summary>The floor tile furthest (squared distance) from <paramref name="from"/>,
    /// ignoring tiles covered by any of <paramref name="avoid"/>.</summary>
    private static Vector2Int FarthestFloor(DungeonTile[,] grid, int width, int depth, Vector2Int from,
        params RectInt[] avoid)
    {
        Vector2Int best = from;
        long bestDist = -1;
        for (int x = 0; x < width; x++)
            for (int z = 0; z < depth; z++)
            {
                if (grid[x, z] != DungeonTile.Floor) continue;
                if (IntersectsAny(x, z, 1, avoid)) continue;
                long dx = x - from.x, dz = z - from.y;
                long dist = dx * dx + dz * dz;
                if (dist > bestDist) { bestDist = dist; best = new Vector2Int(x, z); }
            }
        return best;
    }

    /// <summary>The floor tile farthest from <paramref name="from"/> that can host a
    /// <paramref name="size"/> piece whose doorway lands on that tile (so the piece stays
    /// inside the grid) and that is at least <paramref name="minDistance"/> away, or
    /// (-1, -1) when no tile qualifies. <paramref name="openDoorway"/> additionally requires
    /// floor on all eight neighbours of the doorway tile, so the door can be walked up to.</summary>
    private static Vector2Int FarthestFittingFloor(DungeonTile[,] grid, int width, int depth,
        Vector2Int from, int size, Vector2Int doorway, int minDistance, bool openDoorway)
    {
        long minDist2 = (long)minDistance * minDistance;
        long bestDist = -1;
        Vector2Int best = new Vector2Int(-1, -1);

        // The piece's origin is (tile - doorway), so the doorway tile has to leave room
        // for the piece on every side.
        int minX = Mathf.Max(0, doorway.x), maxX = Mathf.Min(width - 1, width - size + doorway.x);
        int minZ = Mathf.Max(0, doorway.y), maxZ = Mathf.Min(depth - 1, depth - size + doorway.y);
        for (int x = minX; x <= maxX; x++)
            for (int z = minZ; z <= maxZ; z++)
            {
                if (grid[x, z] != DungeonTile.Floor) continue;
                if (openDoorway && (x < 1 || z < 1 || x >= width - 1 || z >= depth - 1)) continue;
                if (openDoorway && !IsAllFloor(grid, x - 1, z - 1, 3)) continue;
                long dx = x - from.x, dz = z - from.y;
                long dist = dx * dx + dz * dz;
                if (dist < minDist2 || dist <= bestDist) continue;
                bestDist = dist;
                best = new Vector2Int(x, z);
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
