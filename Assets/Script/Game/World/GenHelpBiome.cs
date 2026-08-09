using UnityEngine;

public enum BiomeType {Desert, Grass, Forest, Ocean, Mountain}
public class GenHelpBiome : Gen
{
    /// <summary>
    /// Procedural biome layout driven by the branch/loop network and its
    /// Voronoi regions (see GenTopology). Ocean marks the empty sky where
    /// there is no ground.
    /// </summary>
    public static BiomeType GetBiomeType(int x, int z)
        => GenTopology.GetBiome(x, z);
}