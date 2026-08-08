using UnityEngine;

public enum BiomeType {Desert, Grass, Forest}
public class GenHelpBiome : Gen
{
    private static readonly float DrynessOffset = GetDeterministicOffset("BiomeDryness");
    private static readonly float Scale = 0.0025f;

    public static BiomeType GetBiomeType(int x, int z)
    {
        float value = Mathf.PerlinNoise(x * Scale + DrynessOffset, 
            z * Scale + DrynessOffset);
        if (value > 0.6f) return BiomeType.Forest;
        if (value > 0.5f) return BiomeType.Grass;
        return BiomeType.Desert;
    }
}