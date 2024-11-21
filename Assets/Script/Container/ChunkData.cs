using System.Collections.Generic;

[System.Serializable] 
public class ChunkData
{
    public int[,,] Map { get; private set; }
    public int Size { get; private set; } = WorldStatic.CHUNKSIZE;
    public int Depth { get; private set; } = WorldStatic.CHUNKDEPTH;
    public SerializableVector3Int Coordinate { get; private set; }
    public List<EntityData> Entity { get; private set; }

    public ChunkData(SerializableVector3Int coordinate)
    {
        Coordinate = coordinate;
        Map = new int[Size, Depth, Size];
        Entity = new List<EntityData>();
    }
}