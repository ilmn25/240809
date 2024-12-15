using System.Collections.Generic;

[System.Serializable]
public class SerializableChunkData
{
    public int[] map;
    public int size;
    public List<ChunkEntityData> StaticEntity = new List<ChunkEntityData>();
    public List<ChunkEntityData> DynamicEntity = new List<ChunkEntityData>();

    public SerializableChunkData(int size)
    {
        this.size = size;
        map = new int[size * size * size];
    }
    
    public int this[int x, int y, int z]
    {
        get => map[x + size * (y + size * z)];
        set => map[x + size * (y + size * z)] = value;
    }
    
    public ChunkData Deserialize()
    {
        ChunkData data = new ChunkData(size);
        data.Map.array = map;
        data.StaticEntity = StaticEntity;
        data.DynamicEntity = DynamicEntity;
        return data;
    }
}