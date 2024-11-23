using System.Collections.Generic;

[System.Serializable]
public class EntityData
{
    public string ID { get; set; }
    public Dictionary<string, object> Parameters { get; set; }
    public SerializableVector3 Position { get; set; }
    public SerializableVector3Int Bounds { get; set; }
    public EntityType Type { get; set; }

    public EntityData(string id, SerializableVector3 position, SerializableVector3Int bounds = null, Dictionary<string, object> parameters = null, EntityType type = EntityType.Static)
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
    