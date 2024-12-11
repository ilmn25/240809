using System.Collections.Generic;

[System.Serializable]
public class EntityData
{
    public string ID;
    public Dictionary<string, object> Parameters;
    public SerializableVector3Int Position;
    public SerializableVector3Int Bounds;
    public EntityType Type;

    public EntityData(string id, SerializableVector3Int position, SerializableVector3Int bounds = null, EntityType type = EntityType.Static, Dictionary<string, object> parameters = null)
    {
        ID = id;
        Position = position;
        Bounds = bounds ?? new SerializableVector3Int(0, 0, 0);
        Parameters = parameters;
        Type = type;
    }
}

[System.Serializable]
public enum EntityType
{
    Item,
    Static,
    Rigid
}
    