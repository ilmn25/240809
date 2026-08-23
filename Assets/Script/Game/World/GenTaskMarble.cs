using UnityEngine;

public class GenTaskMarble : IGenTask
{
    private static float _x, _z, _value;
    private static int _height;
    private const float Scale = 0.05f;
    private static readonly float Offset = Gen.GetDeterministicOffset("Marble");
    private static int _id;
    
    private static int Marble => _id == 0 ? Block.ConvertID(ID.MarbleBlock) : _id;
    
    public void RunChunk(Vector3Int currentCoordinate, Chunk currentChunk)
    {
        for (int x = 0; x < World.ChunkSize; x++)
        {
            _x = (currentCoordinate.x + x) * Scale + Offset;

            for (int z = 0; z < World.ChunkSize; z++)
            {
                _z = (currentCoordinate.z + z) * Scale + Offset;
                _value = Mathf.PerlinNoise(_x, _z);
                _height = Mathf.FloorToInt(_value * World.ChunkSize);
                
                for (int y = 0; y < World.ChunkSize; y++)
                {
                    if (y + currentCoordinate.y <= _height)
                    {
                        currentChunk[x, y, z] = Marble;
                    }
                }
            }
        }
    }
}