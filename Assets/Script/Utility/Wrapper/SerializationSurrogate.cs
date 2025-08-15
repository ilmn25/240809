using UnityEngine;
using System.Runtime.Serialization;
// connected at Game.cs
public class Vector3IntSerializationSurrogate : ISerializationSurrogate
{
    public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
    {
        Vector3Int v3i = (Vector3Int)obj;
        info.AddValue("x", v3i.x);
        info.AddValue("y", v3i.y);
        info.AddValue("z", v3i.z);
    }

    public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
    {
        Vector3Int v3i = (Vector3Int)obj;
        v3i.x = (int)info.GetValue("x", typeof(int));
        v3i.y = (int)info.GetValue("y", typeof(int));
        v3i.z = (int)info.GetValue("z", typeof(int));
        return v3i;
    }
}

public class Vector3SerializationSurrogate : ISerializationSurrogate
{
    public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
    {
        Vector3 v3 = (Vector3)obj;
        info.AddValue("x", v3.x);
        info.AddValue("y", v3.y);
        info.AddValue("z", v3.z);
    }

    public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
    {
        Vector3 v3 = (Vector3)obj;
        v3.x = (float)info.GetValue("x", typeof(float));
        v3.y = (float)info.GetValue("y", typeof(float));
        v3.z = (float)info.GetValue("z", typeof(float));
        return v3;
    }
}
