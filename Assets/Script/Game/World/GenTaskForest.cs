using UnityEngine;

public class GenTaskForest : Gen
{
    private static int _id;
    private static int Forest => _id == 0 ? Block.ConvertID(ID.ForestBlock) : _id;
    /// <summary>How many blocks deep the forest floor extends below the surface.</summary>
    private const int SurfaceDepth = 3;

    public static void Run(Vector3Int currentCoordinate, Chunk currentChunk)
    {
        for (int x = 0; x < World.ChunkSize; x++)
        {
            for (int z = 0; z < World.ChunkSize; z++)
            {
                if (GenHelpBiome.GetBiomeType(currentCoordinate.x + x, currentCoordinate.z + z) != BiomeType.Forest) continue;

                // Find the highest solid block in this column (the surface).
                int surfaceY = -1;
                for (int y = World.ChunkSize - 1; y >= 0; y--)
                {
                    if (currentChunk[x, y, z] != 0)
                    {
                        surfaceY = y;
                        break;
                    }
                }
                if (surfaceY < 0) continue;

                // Replace the top few blocks with forest floor.
                for (int y = surfaceY; y > surfaceY - SurfaceDepth && y >= 0; y--)
                {
                    currentChunk[x, y, z] = Forest;
                }
            }
        }
    }
}
