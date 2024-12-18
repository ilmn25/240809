using System.Collections.Generic;
using System.IO;


[System.Serializable]
public class Chunk
{
    public Array3D<int> Map;
    public List<ChunkEntityData> StaticEntity;
    public List<ChunkEntityData> DynamicEntity;
    public static Chunk Zero;

    static Chunk()
    {
        Zero = new Chunk();
        for (int x = 0; x < World.ChunkSize; x++)
        {
            for (int y = 0; y < World.ChunkSize; y++)
            {
                for (int z = 0; z < World.ChunkSize; z++)
                {
                    Zero.Map[x, y, z] = 0;
                }
            }
        }
    }

    public Chunk(int size = 0)
    {
        Map = new Array3D<int>();
        Map.Initialize(size == 0 ? World.ChunkSize : size);
        StaticEntity = new List<ChunkEntityData>();
        DynamicEntity = new List<ChunkEntityData>();
    }
 
}