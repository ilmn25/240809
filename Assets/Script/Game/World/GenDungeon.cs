using System.Collections.Generic;
using UnityEngine;

/// <summary>The dungeon dimension: irregular brick rooms (grown socket-to-socket
/// by a Core-Keeper-style layout generator) carved into a stone matrix and
/// linked by doorways, fully enclosed by floor, walls and ceiling.</summary>
public class GenDungeon : Gen
{
    private const int CeilingY = World.ChunkSize - 1;   // 1-block ceiling at the very top layer; interior is y=1..CeilingY-1
    private const int MaxRooms = 70;
    private const float ChestChance = 0.004f;
    private const float RubbleChance = 0.012f;
    private const float OilBarrelChance = 0.006f;
    private const float ArrowTrapChance = 0.008f;
    private const float OldPotChance = 0.01f;

    private static int _stoneId;
    private static int Stone => _stoneId == 0 ? Block.ConvertID(ID.StoneBlock) : _stoneId;
    private static int _brickId;
    private static int Brick => _brickId == 0 ? Block.ConvertID(ID.BrickBlock) : _brickId;

    private static readonly Chunk Exit = SetPiece.LoadSetPieceFile("DungeonExit");

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

        // Place the exit stairwell so its door (set-piece local 6,1,11) sits at
        // the dungeon spawn point.
        if (Exit != null)
        {
            Vector3Int spawn = GetSpawnPoint();
            SetPiece.Paste(world, new Vector3Int(spawn.x - 6, 0, spawn.z - 11), Exit);
        }
    }

    private static void RenderGrid(World world, DungeonTile[,] grid, int width, int depth, int seed)
    {
        System.Random rng = new System.Random(seed ^ 0x5EED);
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                if (grid[x, z] != DungeonTile.Floor) continue;

                SetBlock(world, x, 0, z, Brick);
                for (int y = 1; y < CeilingY; y++)
                    SetBlock(world, x, y, z, 0);
                SetBlock(world, x, CeilingY, z, Brick);
                for (int y = CeilingY + 1; y < World.ChunkSize; y++)
                    SetBlock(world, x, y, z, 0);

                double roll = rng.NextDouble();
                if (roll < ChestChance)
                    PlaceEntity(world, ID.Chest, x, 1, z);
                else if (roll < ChestChance + RubbleChance)
                    PlaceEntity(world, ID.Rubble, x, 1, z);
                else if (roll < ChestChance + RubbleChance + OilBarrelChance)
                    PlaceEntity(world, ID.OilBarrel, x, 1, z);
                else if (roll < ChestChance + RubbleChance + OilBarrelChance + ArrowTrapChance)
                    PlaceEntity(world, ID.ArrowTrap, x, 1, z);
                else if (roll < ChestChance + RubbleChance + OilBarrelChance + ArrowTrapChance + OldPotChance)
                    PlaceEntity(world, ID.OldPot, x, 1, z);
            }
        }
    }

    private static void PlaceEntity(World world, ID id, int x, int y, int z)
    {
        Chunk chunk = ChunkAt(world, x, y, z);
        if (chunk == null) return;
        Info info = Entity.CreateInfo(id, new Vector3Int(x, y, z));
        if (info == null) return;
        chunk.StaticEntity.Add(info);
    }

    private static void SetBlock(World world, int x, int y, int z, int id)
    {
        Chunk chunk = ChunkAt(world, x, y, z);
        if (chunk == null) return;
        chunk[x % World.ChunkSize, y % World.ChunkSize, z % World.ChunkSize] = id;
    }

    private static Chunk ChunkAt(World world, int x, int y, int z)
    {
        Vector3Int pos = new Vector3Int(x, y, z);
        Chunk chunk = world[World.GetChunkCoordinate(pos)];
        return chunk == null || chunk == Chunk.Zero ? null : chunk;
    }
}
