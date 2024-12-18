using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Serialization;


[System.Serializable]
public class Chunk
{
    private int[] _map;
    private int _size;
    public List<ChunkEntityData> staticEntity;
    public List<ChunkEntityData> dynamicEntity;
    public static Chunk Zero; 
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
        _size = size == 0 ? World.ChunkSize : size;
        _map = new int[_size * _size * _size];
        staticEntity = new List<ChunkEntityData>();
        dynamicEntity = new List<ChunkEntityData>();
    } 
    
    public int this[int x, int y, int z]
    {
        get => _map[x + _size * (y + _size * z)];
        set => _map[x + _size * (y + _size * z)] = value;
    }
    
    public int this[Vector3Int position]
    {
        get => _map[position.x + _size * (position.y + _size * position.z)];
        set => _map[position.x + _size * (position.y + _size * position.z)] = value;
    } 
 
}