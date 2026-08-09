using UnityEngine;

/// <summary>
/// Carves bottomless chasms along the ridges where two different biome regions
/// meet (the voronoi boundaries between biome nodes in GenTopology). Land
/// bridges and open sky are left intact, so the bridges span the chasms.
///
/// The walls taper and step inward with depth (crater-style terraces) and are
/// jittered per band with noise, so ravines read as natural ridged canyons
/// instead of straight-sided tubes.
/// </summary>
public class GenTaskRavine : Gen
{
    private static readonly float ErodeOffset = GetDeterministicOffset("RavineErode");
    private static readonly float StepOffset = GetDeterministicOffset("RavineSteps");
    /// <summary>Half-width (blocks) of the ridge band that becomes a chasm.</summary>
    private const float ChasmHalfWidth = 10.5f;
    /// <summary>Number of terrace bands down each wall.</summary>
    private const int RidgeSteps = 5;
    /// <summary>How much narrower the chasm gets at the bottom (fraction of the top).</summary>
    private const float BottomWidthFactor = 0.3f;
    /// <summary>Noise that jitters each wall band so the ridges are jagged.</summary>
    private const float WallNoiseStrength = 2.5f;
    private const float WallNoiseScale = 0.08f;

    public static void Run(Vector3Int currentCoordinate, Chunk currentChunk)
    {
        int worldHeight = World.Inst.Bounds.y;
        float topWidth = ChasmHalfWidth + 2f;
        float bottomWidth = topWidth * BottomWidthFactor;

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

                // Distance from the ridge line (0 = dead centre, grows outward).
                if (!GenTopology.TryGetBiomeBoundaryGap(worldX, worldZ, out float gap)) continue;
                if (gap > topWidth + WallNoiseStrength) continue;

                // Ripples the step boundaries so the terraces undulate down the wall.
                float stepShift = Mathf.PerlinNoise(worldX * 0.06f + StepOffset, worldZ * 0.06f);

                for (int y = 0; y < World.ChunkSize; y++)
                {
                    int worldY = currentCoordinate.y + y;
                    float depth = 1f - (float)worldY / worldHeight; // 0 top, 1 bedrock

                    // Width holds steady per band, then jumps — that makes the ledges.
                    float step = depth * RidgeSteps + stepShift;
                    int band = Mathf.Clamp(Mathf.FloorToInt(step), 0, RidgeSteps);
                    float bandWidth = Mathf.Lerp(topWidth, bottomWidth, (float)band / RidgeSteps);

                    // Jagged wall: each band gets its own noise offset.
                    float wallNoise = (Mathf.PerlinNoise(
                        worldX * WallNoiseScale + ErodeOffset + band,
                        worldZ * WallNoiseScale) - 0.5f) * 2f * WallNoiseStrength;

                    if (gap < bandWidth + wallNoise)
                        currentChunk[x, y, z] = 0;
                }
            }
        }
    }
}
