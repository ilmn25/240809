using UnityEngine;

public class GenTaskMaze : WorldGen
{
    private const int WallHeight = 5;
    private const int FloorHeight = 2;
    
    private static int _id;
    private static int ID => _id == 0 ? Block.ConvertID(global::ID.BackroomBlock) : _id;  

    public static void Run(Vector3Int CurrentCoordinate, Chunk CurrentChunk)
    {
        if (CurrentCoordinate.y != 0) return;

        bool[,] maze = HandleMazeAlgorithm(World.ChunkSize, World.ChunkSize);
        
        for (int y = 0; y < World.ChunkSize; y++)
        {
            if (y > WallHeight + FloorHeight) continue;
            for (int x = 0; x < World.ChunkSize; x++)
            {
                for (int z = 0; z < World.ChunkSize; z++)
                {
                    if (y < FloorHeight)
                    {
                        CurrentChunk[x, y, z] = ID; // Floor
                    }
                    else if (y == WallHeight + FloorHeight)
                    {
                        CurrentChunk[x, y, z] = ID; // Ceiling
                    }
                    else if (maze[x, z] && y < WallHeight + FloorHeight)
                    {
                        CurrentChunk[x, y, z] = ID; // Walls
                    }
                }
            }
        }
    }
    
    
    private static bool[,] HandleMazeAlgorithm(int width, int height)
    {
        bool[,] maze = new bool[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                // Create walls with one block thickness and corridors with four blocks width
                maze[x, z] = (x % 5 == 0 || z % 5 == 0);
            }
        }

        // Randomly remove entire wall sections
        for (int x = 0; x < width; x += 8)
        {
            for (int z = 0; z < height; z += 8)
            {
                if (Random.NextDouble() < 0.8) // 30% chance to remove a wall section
                {
                    // Remove vertical wall section
                    for (int i = 0; i < 8 && x + i < width; i++)
                    {
                        maze[x + i, z] = false;
                    }
                    // Remove horizontal wall section
                    for (int j = 0; j < 8 && z + j < height; j++)
                    {
                        maze[x, z + j] = false;
                    }
                }
            }
        }

        return maze;
    }
}
