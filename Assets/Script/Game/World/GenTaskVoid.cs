using UnityEngine;

/// <summary>
/// Removes all ground outside the landmass, so the world's edge is empty
/// sky with no ground to stand on.
/// </summary>
public class GenTaskVoid : Gen
{
    public static void Run(Vector3Int currentCoordinate, Chunk currentChunk)
    {
        for (int x = 0; x < World.ChunkSize; x++)
        {
            for (int z = 0; z < World.ChunkSize; z++)
            {
                if (GenHelpBiome.GetBiomeType(currentCoordinate.x + x, currentCoordinate.z + z) != BiomeType.Void)
                    continue;

                // Clear the whole column — each chunk clears its own slice.
                for (int y = 0; y < World.ChunkSize; y++)
                    currentChunk[x, y, z] = 0;
            }
        }
    }
}
