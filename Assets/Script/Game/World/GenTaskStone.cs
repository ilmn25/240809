using UnityEngine;

public class GenTaskStone : WorldGen
{
    private static float _x, _z, _value;
    private static int _height;
    private const float Scale = 0.05f;
    private static readonly float Offset = GetOffset();
    private static int _id;
    private static int ID => _id == 0 ? Block.ConvertID("stone") : _id; 
    
    public static void Run()
    {
        for (int x = 0; x < World.ChunkSize; x++)
        {
            _x = Mathf.Abs(CurrentCoordinate.x + x) * Scale + Offset;

            for (int z = 0; z < World.ChunkSize; z++)
            {
                _z = Mathf.Abs(CurrentCoordinate.z + z) * Scale + Offset;
                _value = Mathf.PerlinNoise(_x, _z);
                _height = Mathf.FloorToInt(_value * (WorldHeight / 4)) + (WorldHeight * 3 / 4);
                
                for (int y = 0; y < World.ChunkSize; y++)
                {
                    if (y + CurrentCoordinate.y <= _height - 5)
                    {
                        CurrentChunk[x, y, z] = ID;
                    }
                }
            }
        }
    }
}