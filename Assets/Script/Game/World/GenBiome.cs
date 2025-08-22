using UnityEngine;

public enum BiomeType {Desert, Grass}
public class GenBiome : WorldGen
{
    private static readonly float DrynessOffset = GetOffset();
    private static readonly float Scale = 0.01f;

    public static BiomeType GetBiomeType(int x, int z)
    {
        float value = Mathf.PerlinNoise((CurrentCoordinate.x + x) * Scale + DrynessOffset, 
            (CurrentCoordinate.z + z) * Scale + DrynessOffset);
        if (value > 0.5f) return BiomeType.Grass;
        return BiomeType.Desert;
    }
}