using UnityEngine;

/// <summary>Dungeon dimension: brick rooms (grown socket-to-socket by
/// DungeonLayout) carved into a stone matrix and linked by doorways, enclosed by
/// floor, walls and ceiling. The exit stairwell sits at the spawn point and the
/// Cyclops boss room is pasted at the room farthest from it.</summary>
public class GenDungeon : Gen
{
    private const int CeilingY = World.ChunkSize - 1;   // top brick layer; interior is y=1..CeilingY-1
    private const int MaxRooms = 70;

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

        Vector3Int spawn = GetSpawnPoint();

        // Place the exit stairwell so its door (set-piece local 6,1,11) sits at
        // the dungeon spawn point.
        if (Exit != null)
            SetPiece.Paste(world, new Vector3Int(spawn.x - 6, 0, spawn.z - 11), Exit);

        // Boss room: the floor tile farthest from the exit — a far dead-end of
        // the branching layout (the room blob is connected, so it's reachable).
        if (Boss != null)
        {
            Vector2Int far = FarthestFloor(grid, width, depth, new Vector2Int(spawn.x, spawn.z));
            int ox = Mathf.Clamp(far.x - Boss.size / 2, 0, width - Boss.size);
            int oz = Mathf.Clamp(far.y - Boss.size / 2, 0, depth - Boss.size);
            SetPiece.Paste(world, new Vector3Int(ox, 0, oz), Boss);
        }
    }

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
