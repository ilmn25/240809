using System.Collections.Generic;

[System.Serializable]
public class SerializableChunk
{
    public int[] map;
    public int size;
    public List<ChunkEntityData> StaticEntity = new List<ChunkEntityData>();
    public List<ChunkEntityData> DynamicEntity = new List<ChunkEntityData>();

    public SerializableChunk(int size)
    {
        this.size = size;
        map = new int[size * size * size];
    }
    
    public int this[int x, int y, int z]
    {
        get => map[x + size * (y + size * z)];
        set => map[x + size * (y + size * z)] = value;
    }
    
    public Chunk Deserialize()
    {
        Chunk data = new Chunk(size);
        data.Map.array = map;
        data.StaticEntity = StaticEntity;
        data.DynamicEntity = DynamicEntity;
        return data;
    }
}