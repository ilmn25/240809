using UnityEngine; 

public class GenTaskSand : WorldGen
{
    private static float _x, _z, _value;
    private static int _height;
    private const float Scale = 0.03f;
    private static readonly float Offset = GetOffset();
    private static int _id;
    private static int ID => _id == 0 ? Block.ConvertID(global::ID.SandBlock) : _id; 
    
    public static void Run(Vector3Int CurrentCoordinate, Chunk CurrentChunk)
    {
        for (int x = 0; x < World.ChunkSize; x++)
        {
            _x = (CurrentCoordinate.x + x) * Scale + Offset;

            for (int z = 0; z < World.ChunkSize; z++)
            {
                if (GenBiome.GetBiomeType(CurrentCoordinate.x + x, CurrentCoordinate.z + z) != BiomeType.Desert) continue; 
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