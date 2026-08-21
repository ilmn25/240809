using UnityEngine;

/// <summary>The dungeon dimension: a low-ceilinged brick maze, fully enclosed
/// (floor + ceiling) so it reads as an underground lair.</summary>
public class GenDungeon : Gen
{
    private const int WallHeight = 3;
    private const int FloorHeight = 1;

    private static int _brickId;
    private static int Brick => _brickId == 0 ? Block.ConvertID(ID.BrickBlock) : _brickId;

    public override Vector3Int GetSize() => new Vector3Int(20, 4, 20);
    public override Vector3Int GetSpawnPoint() => new Vector3Int(GetSize().x / 2, GetSize().y - 2, GetSize().z / 2) * World.ChunkSize;

    protected override void GenChunk(Vector3Int currentCoordinate, Chunk currentChunk)
    {
        if (currentCoordinate.y != 0) return;

        System.Random rng = CreateChunkRandom("Dungeon", currentCoordinate);
        bool[,] maze = HandleMazeAlgorithm(World.ChunkSize, World.ChunkSize, rng);

        for (int y = 0; y < World.ChunkSize; y++)
        {
            if (y > WallHeight + FloorHeight) continue;
            for (int x = 0; x < World.ChunkSize; x++)
            {
                for (int z = 0; z < World.ChunkSize; z++)
                {
                    if (y < FloorHeight)
                        currentChunk[x, y, z] = Brick;              // floor
                    else if (y == WallHeight + FloorHeight)
                        currentChunk[x, y, z] = Brick;              // ceiling
                    else if (maze[x, z])
                        currentChunk[x, y, z] = Brick;              // walls
                    else
                        currentChunk[x, y, z] = 0;                  // air
                }
            }
        }
    }

    private static bool[,] HandleMazeAlgorithm(int width, int height, System.Random rng)
    {
        bool[,] maze = new bool[width, height];

        for (int x = 0; x < width; x++)
            for (int z = 0; z < height; z++)
                maze[x, z] = (x % 5 == 0 || z % 5 == 0);

        // Randomly punch doorways through the walls.
        for (int x = 0; x < width; x += 8)
        {
            for (int z = 0; z < height; z += 8)
            {
                if (rng.NextDouble() < 0.8)
                {
                    for (int i = 0; i < 8 && x + i < width; i++)
                        maze[x + i, z] = false;
                    for (int j = 0; j < 8 && z + j < height; j++)
                        maze[x, z + j] = false;
                }
            }
        }

        return maze;
    }
}
