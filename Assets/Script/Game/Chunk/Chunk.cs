using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class Chunk 
{
    protected int[] Map;
    protected int Size;  
    
    public static readonly Chunk Zero;   
    public readonly List<Info> StaticEntity;
    public readonly List<Info> DynamicEntity; 
    
    static Chunk()
    {
        Zero = new Chunk();
        for (int x = 0; x < World.ChunkSize; x++)
        {
            for (int y = 0; y < World.ChunkSize; y++)
            {
                for (int z = 0; z < World.ChunkSize; z++)
                {
                    Zero[x, y, z] = 0;
                }
            }
        }
    } 
    public Chunk(int size = 0)
    {
        Size = size == 0 ? World.ChunkSize : size;
        Map = new int[Size * Size * Size];
        StaticEntity = new List<Info>();
        DynamicEntity = new List<Info>();
    } 
    
    public int this[int x, int y, int z]
    {
        get => Map[x + Size * (y + Size * z)];
        set => Map[x + Size * (y + Size * z)] = value;
    }
    
    public int this[Vector3Int position]
    {
        get => Map[position.x + Size * (position.y + Size * position.z)];
        set => Map[position.x + Size * (position.y + Size * position.z)] = value;
    } 
} 