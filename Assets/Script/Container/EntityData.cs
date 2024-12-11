using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChunkEntityData
{
    public string stringID;
    public Dictionary<string, object> Parameters;
    public SerializableVector3Int position;
    public ChunkEntityData(string stringID, SerializableVector3Int position)
    {
        this.stringID = stringID;
        this.position = position; 
    }
}

public class EntityData
{
    public Dictionary<string, object> Parameters;
    public Vector3Int Bounds;
    public EntityType Type;
    public static EntityData Zero = new EntityData();
    public static EntityData One = new EntityData(Vector3Int.one);
    public static EntityData Rigid = new EntityData(type: EntityType.Rigid);
    public EntityData(Vector3Int? bounds = null, EntityType type = EntityType.Static, Dictionary<string, object> parameters = null)
    {
        Bounds = bounds ?? Vector3Int.zero;
        Parameters = parameters;
        Type = type;
    }
}

public enum EntityType
{
    Item,
    Static,
    Rigid
}
    