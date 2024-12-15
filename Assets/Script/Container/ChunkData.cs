using System.Collections.Generic;
using System.IO;


[System.Serializable]
public class ChunkData
{
    public Array3D<int> Map;
    public List<ChunkEntityData> StaticEntity;
    public List<ChunkEntityData> DynamicEntity;
    public static ChunkData Zero;

    static ChunkData()
    {
        Zero = new ChunkData();
        for (int x = 0; x < WorldSingleton.CHUNK_SIZE; x++)
        {
            for (int y = 0; y < WorldSingleton.CHUNK_SIZE; y++)
            {
                for (int z = 0; z < WorldSingleton.CHUNK_SIZE; z++)
                {
                    Zero.Map[x, y, z] = 0;
                }
            }
        }
    }

    public ChunkData(int size = 0)
    {
        Map = new Array3D<int>();
        Map.Initialize(size == 0 ? WorldSingleton.CHUNK_SIZE : size);
        StaticEntity = new List<ChunkEntityData>();
        DynamicEntity = new List<ChunkEntityData>();
    }
 
}