using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Serialization;

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
} 