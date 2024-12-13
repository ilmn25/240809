using System;
using UnityEngine;

[System.Serializable]
public class WorldData
{
    private ChunkData[,,] _world;  
    public SerializableVector3Int Length;
    public SerializableVector3Int Bounds;

    // Constructor to initialize the World array
    public WorldData(int x, int y, int z)
    {
        Bounds = new SerializableVector3Int(x * WorldSingleton.CHUNK_SIZE, y * WorldSingleton.CHUNK_SIZE, z * WorldSingleton.CHUNK_SIZE);
        Length = new SerializableVector3Int(x, y, z);
        _world = new ChunkData[x, y, z];
    }
 
    public ChunkData this[Vector3Int position]
    {
        
        get
        {
            int chunkX = position.x / WorldSingleton.CHUNK_SIZE;
            int chunkY = position.y / WorldSingleton.CHUNK_SIZE;
            int chunkZ = position.z / WorldSingleton.CHUNK_SIZE;

            if (chunkX >= 0 && chunkX < _world.GetLength(0) &&
                chunkY >= 0 && chunkY < _world.GetLength(1) &&
                chunkZ >= 0 && chunkZ < _world.GetLength(2))
            {
                return _world[chunkX, chunkY, chunkZ];
            }
            else
            {
                return ChunkData.Zero;
            }
        }
        set
        {
            int chunkX = position.x / WorldSingleton.CHUNK_SIZE;
            int chunkY = position.y / WorldSingleton.CHUNK_SIZE;
            int chunkZ = position.z / WorldSingleton.CHUNK_SIZE; 
            if (chunkX >= 0 && chunkX < _world.GetLength(0) &&
                chunkY >= 0 && chunkY < _world.GetLength(1) &&
                chunkZ >= 0 && chunkZ < _world.GetLength(2))
            {
                _world[chunkX, chunkY, chunkZ] = value;
            }
            else
            {
                throw new IndexOutOfRangeException("Coordinates are out of bounds.");
            }
        }
    }
    
    public ChunkData this[int x, int y, int z]
    {
        get
        {
            int chunkX = x / WorldSingleton.CHUNK_SIZE;
            int chunkY = y / WorldSingleton.CHUNK_SIZE;
            int chunkZ = z / WorldSingleton.CHUNK_SIZE;

            if (chunkX >= 0 && chunkX < _world.GetLength(0) &&
                chunkY >= 0 && chunkY < _world.GetLength(1) &&
                chunkZ >= 0 && chunkZ < _world.GetLength(2))
            {
                return _world[chunkX, chunkY, chunkZ];
            }
            else
            {
                return ChunkData.Zero;
            }
        }
        set
        {
            int chunkX = x / WorldSingleton.CHUNK_SIZE;
            int chunkY = y / WorldSingleton.CHUNK_SIZE;
            int chunkZ = z / WorldSingleton.CHUNK_SIZE; 
            if (chunkX >= 0 && chunkX < _world.GetLength(0) &&
                chunkY >= 0 && chunkY < _world.GetLength(1) &&
                chunkZ >= 0 && chunkZ < _world.GetLength(2))
            {
                _world[chunkX, chunkY, chunkZ] = value;
            }
            else
            {
                throw new IndexOutOfRangeException("Coordinates are out of bounds.");
            }
        }
    }
}