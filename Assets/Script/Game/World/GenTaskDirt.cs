using UnityEngine;

public class GenTaskDirt : WorldGen
{
    private static float _x, _z, _value;
    private static int _height;
    private const float Scale = 0.05f;
    private static readonly float Offset = GetOffset();
    private static int _id;
    
    private static int Dirt => _id == 0 ? Block.ConvertID(ID.DirtBlock) : _id;
    private const int VerticalScale  = World.ChunkSize;
    
    public static void Run(Vector3Int currentCoordinate, Chunk currentChunk)
    {
        for (int x = 0; x < World.ChunkSize; x++)
        {
            _x = (currentCoordinate.x + x) * Scale + Offset;

            for (int z = 0; z < World.ChunkSize; z++)
            { 
                if (GenBiome.GetBiomeType(currentCoordinate.x + x, currentCoordinate.z + z) != BiomeType.Grass) continue; 
                _z = (currentCoordinate.z + z) * Scale + Offset;

                _value = Mathf.PerlinNoise(_x, _z);
                _height = Mathf.FloorToInt(_value * VerticalScale + WorldHeight);
                
                for (int y = 0; y < World.ChunkSize; y++)
                {
                    if (currentChunk[x, y, z] != 0 &&
                        y + currentCoordinate.y > _height - 15)
                    {
                        currentChunk[x, y, z] = Dirt;
                    }
                }
            }
        }
    }
}