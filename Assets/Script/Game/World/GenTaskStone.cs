using System.Drawing;
using UnityEngine;

public class GenTaskStone : Gen
{
    private static float _x, _z, _value;
    private static int _height;
    private const float Scale = 0.05f;
    private static readonly float Offset = GetOffset();
    private static int _id;
    private static int Stone => _id == 0 ? Block.ConvertID(ID.StoneBlock) : _id;
    private const int VerticalScale  = World.ChunkSize;
    
    public static void Run(Vector3Int currentCoordinate, Chunk currentChunk)
    {
        for (int x = 0; x < World.ChunkSize; x++)
        {
            _x = Mathf.Abs(currentCoordinate.x + x) * Scale + Offset;

            for (int z = 0; z < World.ChunkSize; z++)
            {
                _z = Mathf.Abs(currentCoordinate.z + z) * Scale + Offset;
                _value = Mathf.PerlinNoise(_x, _z);
                _height = Mathf.FloorToInt(_value * VerticalScale + WorldHeight);
                
                for (int y = 0; y < World.ChunkSize; y++)
                {
                    if (y + currentCoordinate.y <= _height - 5)
                    {
                        currentChunk[x, y, z] = Stone;
                    }
                }
            }
        }
    }
}