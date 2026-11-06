using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Chunk 
{
    public static readonly Chunk Zero = new();   
    
    protected int[] Map;
    public int size;     
    public readonly List<Info> StaticEntity = new();
    public readonly List<Info> DynamicEntity = new();
     
    public Chunk(int size = 0)
    {
        this.size = size == 0 ? World.ChunkSize : size;
        Map = new int[this.size * this.size * this.size]; 
    } 
    
    public int this[int x, int y, int z]
    {
        get => Map[x + size * (y + size * z)];
        set => Map[x + size * (y + size * z)] = value;
    }
    
    public int this[Vector3Int position]
    {
        get => Map[position.x + size * (position.y + size * position.z)];
        set => Map[position.x + size * (position.y + size * position.z)] = value;
    } 

    /// <summary>Record a generated entity in this chunk: registered ids persist as static
    /// entities, loose items (unregistered ids) as dynamic ones. Returns null when the id
    /// has no info.</summary>
    public Info PlaceEntity(ID id, Vector3Int cell)
    {
        Info info = Entity.CreateInfo(id, cell);
        if (info == null) return null;
        (Entity.Dictionary.ContainsKey(id) ? StaticEntity : DynamicEntity).Add(info);
        return info;
    }
} 