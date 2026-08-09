using UnityEngine;

/// <summary>
/// Carves bottomless chasms along the ridges where two different biome regions
/// meet (the voronoi boundaries between biome nodes in GenTopology). Roads and
/// open sky are left intact, so the dirt roads become bridges spanning chasms.
/// </summary>
public class GenTaskRavine : Gen
{
    private static readonly float ErodeOffset = GetDeterministicOffset("RavineErode");
    /// <summary>Half-width (blocks) of the ridge band that becomes a chasm.</summary>
    private const float ChasmHalfWidth = 10.5f;
    /// <summary>Noise that perturbs the chasm edge so it looks eroded, not straight.</summary>
    private const float ErodeStrength = 4f;
    private const float ErodeScale = 0.08f;

    public static void Run(Vector3Int currentCoordinate, Chunk currentChunk)
    {
        for (int x = 0; x < World.ChunkSize; x++)
        {
            for (int z = 0; z < World.ChunkSize; z++)
            {
                int worldX = currentCoordinate.x + x;
                int worldZ = currentCoordinate.z + z;

                // Leave open sky intact.
                if (GenHelpBiome.GetBiomeType(worldX, worldZ) == BiomeType.Ocean) continue;

                // Never cut the land bridges — they keep the biome islands connected.
                if (GenTopology.IsBridge(worldX, worldZ)) continue;

                // Erode the chasm edge with noise so it's jagged, not straight.
                float erosion = (Mathf.PerlinNoise(worldX * ErodeScale + ErodeOffset,
                    worldZ * ErodeScale + ErodeOffset) - 0.5f) * 2f * ErodeStrength;

                // Only carve columns near a biome-boundary ridge.
                if (!GenTopology.IsBiomeBoundary(worldX, worldZ, ChasmHalfWidth + erosion))
                    continue;

                // Clear the whole column — a bottomless chasm down to bedrock.
                // Each chunk clears its own slice, so chasms span all chunk heights.
                for (int y = 0; y < World.ChunkSize; y++)
                    currentChunk[x, y, z] = 0;
            }
        }
    }
}
