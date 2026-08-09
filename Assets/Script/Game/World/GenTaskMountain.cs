using UnityEngine;

/// <summary>
/// Builds the Mountain biome: cascading stone cliffs that rise in big flat
/// ledges toward each mountain node — the same staircase idea as the crater,
/// but upward. Runs after the biome surfaces so the peaks stay rocky, and
/// before voids/ravines so those still cut through them.
/// </summary>
public class GenTaskMountain : Gen
{
    private const int Steps = 5;               // cliff ledges up each mountain
    private const float PeakScale = 2f;        // max peak height as a multiple of chunk size
    private const float RadiusScale = 3f;      // footprint radius as a multiple of chunk size
    private const int DirtCap = 2;             // grassy top thickness in blocks
    private const float NoiseStrength = 4f;
    private const float NoiseScale = 0.02f;
    private static readonly float Offset = GetDeterministicOffset("Mountain");

    private static Vector3Int[] _centers;
    private static float[] _heightScale;       // per-mountain peak height variation
    private static int _radius;

    private static void EnsureCenters()
    {
        if (_centers != null) return;
        _radius = Mathf.FloorToInt(RadiusScale * World.ChunkSize);
        _centers = GenTopology.GetMountainCenters();
        _heightScale = new float[_centers.Length];
        for (int i = 0; i < _centers.Length; i++)
            _heightScale[i] = 0.8f + 0.7f * Mathf.PerlinNoise(Offset + i * 13.7f, Offset + i * 7.9f);
    }

    public static void Run(Vector3Int currentCoordinate, Chunk currentChunk)
    {
        EnsureCenters();
        if (_centers == null || _centers.Length == 0) return;
        int chunkSize = World.ChunkSize;
        int stone = Block.ConvertID(ID.StoneBlock);
        int dirt = Block.ConvertID(ID.GrassBlock);
        float peak = PeakScale * chunkSize;
        float cliffHeight = peak / Steps;
        float bandWidth = _radius / (float)Steps;

        for (int x = 0; x < chunkSize; x++)
        {
            for (int z = 0; z < chunkSize; z++)
            {
                int worldX = currentCoordinate.x + x;
                int worldZ = currentCoordinate.z + z;
                if (GenHelpBiome.GetBiomeType(worldX, worldZ) != BiomeType.Mountain) continue;

                float peakY = 0;
                for (int i = 0; i < _centers.Length; i++)
                {
                    Vector3Int c = _centers[i];
                    float dist = Mathf.Sqrt((worldX - c.x) * (worldX - c.x) + (worldZ - c.z) * (worldZ - c.z));
                    peakY = Mathf.Max(peakY, MountainHeight(dist, peak * _heightScale[i], cliffHeight, bandWidth, worldX, worldZ));
                }
                if (peakY <= 0) continue;

                // Fill air cells up to the peak — solid terrain below stays put.
                // Stone cliffs with a grassy cap on the summit.
                for (int y = 0; y < chunkSize; y++)
                {
                    int worldY = currentCoordinate.y + y;
                    if (worldY > peakY || currentChunk[x, y, z] != 0) continue;
                    currentChunk[x, y, z] = worldY > peakY - DirtCap ? dirt : stone;
                }
            }
        }
    }

    // Cascading cliffs: each band is a flat plateau (constant height) and the
    // drop between bands is a vertical cliff. The distance is blobbed so the
    // footprint isn't a clean circle, cliff height varies per column so the
    // terraces aren't uniform, and the summit is carved into a broken peak.
    private static float MountainHeight(float dist, float peak, float cliffHeight, float bandWidth, int worldX, int worldZ)
    {
        float shape = Mathf.PerlinNoise(worldX * NoiseScale + Offset, worldZ * NoiseScale) - 0.5f;
        float distorted = dist + shape * 2f * _radius * 0.3f;

        float cliffScale = 0.7f + Mathf.PerlinNoise(worldX * NoiseScale * 2f + Offset, worldZ * NoiseScale * 2f);

        int band = Mathf.Clamp(Mathf.FloorToInt(distorted / bandWidth), 0, Steps);
        float height = peak - band * cliffHeight * cliffScale;

        // Break the flat plateau top into a jagged summit.
        float summit = Mathf.PerlinNoise(worldX * NoiseScale * 3f + Offset, worldZ * NoiseScale * 3f);
        if (band == 0)
            height -= summit * peak * 0.35f;

        float edge = (Mathf.PerlinNoise(worldX * NoiseScale * 2f + Offset + band * 0.37f, worldZ * NoiseScale * 2f) - 0.5f) * NoiseStrength;
        return Mathf.Max(0f, height + edge);
    }
}
