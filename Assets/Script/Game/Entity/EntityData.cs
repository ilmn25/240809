using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChunkEntityData
{
    public string stringID;
    public SVector3Int position;
}
public interface IEntity
{
    Vector3Int Bounds { get; }
    EntityType Type { get; }

    public ChunkEntityData GetChunkEntityData(string stringID, SVector3Int position);
}

public partial class Entity<T> : IEntity where T : ChunkEntityData, new()
{
    public Vector3Int Bounds { get; set; }
    public EntityType Type { get; set; }
    public static Entity<ChunkEntityData> Zero = new Entity<ChunkEntityData>();
    public static Entity<ChunkEntityData> One = new Entity<ChunkEntityData>(bounds: Vector3Int.one);
    public static Entity<ChunkEntityData> Item = new Entity<ChunkEntityData>(type: EntityType.Item);

    public Entity(Vector3Int? bounds = null, EntityType type = EntityType.Static)
    {
        Bounds = bounds ?? Vector3Int.zero;
        Type = type;
    }

    public ChunkEntityData GetChunkEntityData(string stringID, SVector3Int position)
    {
        ChunkEntityData data = new T();
        data.position = position;
        data.stringID = stringID;
        return data;
    }
}

public enum EntityType
{
    Item,
    Static,
    Rigid
}
    