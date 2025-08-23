using UnityEngine;

public class GenTaskMarble : WorldGen
{
    private static float _x, _z, _value;
    private static int _height;
    private const float Scale = 0.05f;
    private static readonly float Offset = GetOffset();
    private static int _id;
    private static int ID => _id == 0 ? Block.ConvertID(global::ID.MarbleBlock) : _id; 
    
    public static void Run(Vector3Int CurrentCoordinate, Chunk CurrentChunk)
    {
        for (int x = 0; x < World.ChunkSize; x++)
        {
            _x = (CurrentCoordinate.x + x) * Scale + Offset;

            for (int z = 0; z < World.ChunkSize; z++)
            {
                _z = (CurrentCoordinate.z + z) * Scale + Offset;
                _value = Mathf.PerlinNoise(_x, _z);
                _height = Mathf.FloorToInt(_value * (WorldHeight / 4));
                
                for (int y = 0; y < World.ChunkSize; y++)
                {
                    if (y + CurrentCoordinate.y <= _height)
                    {
                        CurrentChunk[x, y, z] = ID;
                    }
                }
            }
        }
    }
}