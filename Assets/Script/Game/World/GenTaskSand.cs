using UnityEngine;
public enum BiomeType {Desert, Grass}
public class GenBiome : WorldGen
{
    protected static float DrynessOffset = GetOffset();
    protected static float Scale = 0.02f;

    public static BiomeType GetBiomeType(int x, int z)
    {
        float value = Mathf.PerlinNoise((CurrentCoordinate.x + x) * Scale + DrynessOffset, 
            (CurrentCoordinate.z + z) * Scale + DrynessOffset);
        if (value > 0.5f) return BiomeType.Grass;
        return BiomeType.Desert;
    }
}
public class GenTaskSand : WorldGen
{
    private static float _x, _z, _value;
    private static int _height;
    private const float Scale = 0.03f;
    private static readonly float Offset = GetOffset();
    private static int _id;
    private static int ID => _id == 0 ? Block.ConvertID("sand") : _id; 
    
    public static void Run()
    {
        for (int x = 0; x < World.ChunkSize; x++)
        {
            _x = (CurrentCoordinate.x + x) * Scale + Offset;

            for (int z = 0; z < World.ChunkSize; z++)
            {
                if (GenBiome.GetBiomeType(x, z) != BiomeType.Desert) continue; 
                _z = (CurrentCoordinate.z + z) * Scale + Offset;
                _value = Mathf.PerlinNoise(_x, _z);
                _height = Mathf.FloorToInt(_value * (WorldHeight / 4)) + WorldHeight * 3 / 4;
                
                for (int y = 0; y < World.ChunkSize; y++)
                {
                    if (CurrentChunk[x, y, z] != 0 &&
                        y + CurrentCoordinate.y > _height - 15)
                    {
                        CurrentChunk[x, y, z] = ID;
                    }
                }
            }
        }
    }
}