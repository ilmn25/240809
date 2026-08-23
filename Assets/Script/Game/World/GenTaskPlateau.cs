using UnityEngine;

/// <summary>
/// Replaces the void with a low plateau landmass. Every void column is filled
/// with a raised stone-and-grass plateau (a few blocks tall — blocks walking
/// but not the camera, and never traps the player). Biome-boundary ridges also
/// get a low cliff. Land connections and the spawn hub are left open so the
/// world stays traversable.
/// </summary>
public static class GenTaskPlateau
{
    private const float NoiseStrength = 4f;
    private const float NoiseScale = 0.02f;
    private static readonly float Offset = Gen.GetDeterministicOffset("Plateau");
    /// <summary>Plateau height in blocks — blocks walking but not the camera.</summary>
    private const float CliffHeight = 10f;
    /// <summary>Radius (blocks) around the spawn hub that is never raised.</summary>
    private const float SpawnClearRadius = 12f;
    /// <summary>Half-width (blocks) of the ridge band that becomes a cliff.</summary>
    private const float CliffHalfWidth = 20f;

    public static void Run(Vector3Int currentCoordinate, Chunk currentChunk)
    {
        int chunkSize = World.ChunkSize;
        int stone = Block.ConvertID(ID.StoneBlock);
        int dirt = Block.ConvertID(ID.GrassBlock);

        for (int x = 0; x < chunkSize; x++)
        {
            for (int z = 0; z < chunkSize; z++)
            {
                int worldX = currentCoordinate.x + x;
                int worldZ = currentCoordinate.z + z;
                BiomeType biome = GenHelpBiome.GetBiomeType(worldX, worldZ);

                // Never block the spawn hub — the player must always land on solid ground.
                float spawnDx = worldX - World.Inst.SpawnPoint.x;
                float spawnDz = worldZ - World.Inst.SpawnPoint.z;
                if (spawnDx * spawnDx + spawnDz * spawnDz < SpawnClearRadius * SpawnClearRadius) continue;

                // Never block the land connections — they keep the biome plates
                // connected into a walkable ring.
                if (GenTopology.IsLandConnection(worldX, worldZ)) continue;

                // Build on void columns (replacing the empty sky) OR on the
                // biome-boundary ridges (a low cliff between biomes).
                bool isVoid = biome == BiomeType.Void;
                bool isBoundary = !isVoid &&
                    GenTopology.TryGetBiomeBoundaryGap(worldX, worldZ, out float gap) &&
                    gap <= CliffHalfWidth;
                if (!isVoid && !isBoundary) continue;

                // Find the existing surface height in this column (the highest
                // solid block), so the plateau rises above it instead of being
                // buried at the same height as the surrounding terrain.
                int surfaceY = -1;
                for (int y = chunkSize - 1; y >= 0; y--)
                {
                    if (currentChunk[x, y, z] != 0) { surfaceY = currentCoordinate.y + y; break; }
                }
                if (surfaceY < 0) continue;

                // Jagged edge so the plateau isn't a straight wall.
                float edge = (Mathf.PerlinNoise(worldX * NoiseScale + Offset, worldZ * NoiseScale) - 0.5f) * NoiseStrength;
                float height = surfaceY + CliffHeight + edge;

                // Raise the plateau: fill air cells up to the height. Solid terrain
                // below stays put. Stone with a grassy cap on top.
                for (int y = 0; y < chunkSize; y++)
                {
                    int worldY = currentCoordinate.y + y;
                    if (worldY > height || currentChunk[x, y, z] != 0) continue;
                    currentChunk[x, y, z] = worldY > height - 1 ? dirt : stone;
                }
            }
        }
    }
}
