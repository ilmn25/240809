using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChunkEntityData
{
    public string stringID;
    public SerializableVector3Int position;
}
public interface IEntityData
{
    Vector3Int Bounds { get; }
    EntityType Type { get; }

    public ChunkEntityData GetChunkEntityData(string stringID, SerializableVector3Int position);
}

public class EntityData<T> : IEntityData where T : ChunkEntityData, new()
{
    public Vector3Int Bounds { get; set; }
    public EntityType Type { get; set; }
    public static EntityData<ChunkEntityData> Zero = new EntityData<ChunkEntityData>();
    public static EntityData<ChunkEntityData> One = new EntityData<ChunkEntityData>(bounds: Vector3Int.one);
    public static EntityData<ChunkEntityData> Item = new EntityData<ChunkEntityData>(type: EntityType.Item);

    public EntityData(Vector3Int? bounds = null, EntityType type = EntityType.Static)
    {
        Bounds = bounds ?? Vector3Int.zero;
        Type = type;
    }

    public ChunkEntityData GetChunkEntityData(string stringID, SerializableVector3Int position)
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
    