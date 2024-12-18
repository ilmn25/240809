using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class World
{
    public delegate void Vector3IntEvent(Vector3Int position);
    public static event Vector3IntEvent MapUpdated;
    [NonSerialized] public static World Inst;
    [NonSerialized] public static readonly int ChunkSize = 15;
     
    private Chunk[] _chunks;
    public readonly SVector3Int Length;
    public readonly SVector3Int Bounds;
    public readonly int Seed;
 

    public World(int x, int y, int z)
    {
        Bounds = new SVector3Int(x * ChunkSize, y * ChunkSize, z * ChunkSize);
        Length = new SVector3Int(x, y, z);
        _chunks = new Chunk[x * y * z];
        Seed = Environment.TickCount;
    }

    public static void Save(int id)
    {
        EntityStaticLoad.UnloadWorld();
        EntityDynamicLoad.UnloadWorld();

        Utility.Save(Inst, "World" + id);
    }

    public static void Load(int id)
    { 
        Inst = Utility.Load<World>("World" + id);
        if (Inst == null)
            WorldGen.GenerateTestMap();
    }

    private int GetIndex(int chunkX, int chunkY, int chunkZ)
    {
        return chunkX + Length.x * (chunkY + Length.y * chunkZ);
    }

    private bool IsInRange(int chunkX, int chunkY, int chunkZ)
    {
        return chunkX >= 0 && chunkX < Length.x &&
               chunkY >= 0 && chunkY < Length.y &&
               chunkZ >= 0 && chunkZ < Length.z;
    }

    public Chunk this[Vector3Int position]
    {
        get
        {
            position /= ChunkSize;
            if (IsInRange(position.x, position.y, position.z))
                return _chunks[GetIndex(position.x, position.y, position.z)];
            return Chunk.Zero;  
        }
        set
        {
            position /= ChunkSize;
            if (IsInRange(position.x, position.y, position.z))
                _chunks[GetIndex(position.x, position.y, position.z)] = value;
            else
                throw new IndexOutOfRangeException("Coordinates are out of bounds.");
        }
    }

    public Chunk this[int x, int y, int z]
    {
        get
        {
            int chunkX = x / ChunkSize;
            int chunkY = y / ChunkSize;
            int chunkZ = z / ChunkSize;

            if (IsInRange(chunkX, chunkY, chunkZ))
                return _chunks[GetIndex(chunkX, chunkY, chunkZ)];
            return Chunk.Zero; 
        }
        set
        {
            int chunkX = x / ChunkSize;
            int chunkY = y / ChunkSize;
            int chunkZ = z / ChunkSize;

            if (IsInRange(chunkX, chunkY, chunkZ))
                _chunks[GetIndex(chunkX, chunkY, chunkZ)] = value;
            else
                throw new IndexOutOfRangeException("Coordinates are out of bounds.");
        }
    }
    
    public static Boolean IsInWorldBounds(Vector3 worldPosition)
    {
        if (worldPosition.x < Inst.Bounds.x && worldPosition.x >= 0 &&
            worldPosition.y < Inst.Bounds.y && worldPosition.y >= 0 &&
            worldPosition.z < Inst.Bounds.z && worldPosition.z >= 0)
            return true;
        return false;
    }

    public static Boolean IsInWorldBounds(int x, int y, int z)
    {
        if (x < Inst.Bounds.x && x >= 0 &&
            y < Inst.Bounds.y && y >= 0 &&
            z < Inst.Bounds.z && z >= 0)
            return true;
        return false;
    }

    public static Vector3Int GetChunkCoordinate(Vector3 coordinate) 
    {
        Vector3Int chunkPosition = new Vector3Int(
            (int)Mathf.Floor(coordinate.x / ChunkSize) * ChunkSize, 
            (int)Mathf.Floor(coordinate.y / ChunkSize) * ChunkSize, 
            (int)Mathf.Floor(coordinate.z / ChunkSize) * ChunkSize);
        return chunkPosition;
    }

    public static Vector3Int GetBlockCoordinate(Vector3 coordinate) 
    {
        Vector3Int chunkPosition = GetChunkCoordinate(coordinate);

        Vector3 blockPositionInChunk = new Vector3(
            coordinate.x - chunkPosition.x, 
            coordinate.y - chunkPosition.y, 
            coordinate.z - chunkPosition.z);

        return Vector3Int.FloorToInt(blockPositionInChunk);
    } 
 
    public static int GetBlock(Vector3Int coordinate) //0 = empty, 1 = block, error = out of bounds
    { 
        return Inst[GetChunkCoordinate(coordinate)][GetBlockCoordinate(coordinate)];
    }

    public static void SetBlock(Vector3Int coordinate, int blockID = 0)
    {
        Vector3Int blockCoordinate = GetBlockCoordinate(coordinate);
        Vector3Int chunkCoordinate = GetChunkCoordinate(coordinate);
         
        Inst[chunkCoordinate][blockCoordinate] = blockID;
        NavMap.Set(coordinate, blockID == 0);
        
        if (MapUpdated != null) MapUpdated(coordinate);
        
        MapLoad.RefreshExistingChunk(chunkCoordinate); // Refresh on screen
        if (blockCoordinate.x != 0 && blockCoordinate.x != ChunkSize - 1 &&
            blockCoordinate.y != 0 && blockCoordinate.y != ChunkSize - 1 &&
            blockCoordinate.z != 0 && blockCoordinate.z != ChunkSize - 1)
            return;
        
        // X-axis checks
        if (blockCoordinate.x == 0)
            MapLoad.RefreshExistingChunk(Utility.AddToVector(chunkCoordinate, -ChunkSize, 0, 0));
        else if (blockCoordinate.x == ChunkSize - 1)
            MapLoad.RefreshExistingChunk(Utility.AddToVector(chunkCoordinate, ChunkSize, 0, 0));

        // Y-axis checks
        if (blockCoordinate.y == 0)
            MapLoad.RefreshExistingChunk(Utility.AddToVector(chunkCoordinate, 0, -ChunkSize, 0));
        else if (blockCoordinate.y == ChunkSize - 1)
            MapLoad.RefreshExistingChunk(Utility.AddToVector(chunkCoordinate, 0, ChunkSize, 0));

        // Z-axis checks
        if (blockCoordinate.z == 0)
            MapLoad.RefreshExistingChunk(Utility.AddToVector(chunkCoordinate, 0, 0, -ChunkSize));
        else if (blockCoordinate.z == ChunkSize - 1)
            MapLoad.RefreshExistingChunk(Utility.AddToVector(chunkCoordinate, 0, 0, ChunkSize));

        // Edge and Corner checks on X, Y, and Z axes
        if (blockCoordinate.x == 0 && blockCoordinate.y == 0 && blockCoordinate.z == 0)
            MapLoad.RefreshExistingChunk(Utility.AddToVector(chunkCoordinate, -ChunkSize, -ChunkSize, -ChunkSize));
        else if (blockCoordinate.x == 0 && blockCoordinate.y == 0 && blockCoordinate.z == ChunkSize - 1)
            MapLoad.RefreshExistingChunk(Utility.AddToVector(chunkCoordinate, -ChunkSize, -ChunkSize, ChunkSize));
        else if (blockCoordinate.x == 0 && blockCoordinate.y == ChunkSize - 1 && blockCoordinate.z == 0)
            MapLoad.RefreshExistingChunk(Utility.AddToVector(chunkCoordinate, -ChunkSize, ChunkSize, -ChunkSize));
        else if (blockCoordinate.x == 0 && blockCoordinate.y == ChunkSize - 1 && blockCoordinate.z == ChunkSize - 1)
            MapLoad.RefreshExistingChunk(Utility.AddToVector(chunkCoordinate, -ChunkSize, ChunkSize, ChunkSize));
        else if (blockCoordinate.x == ChunkSize - 1 && blockCoordinate.y == 0 && blockCoordinate.z == 0)
            MapLoad.RefreshExistingChunk(Utility.AddToVector(chunkCoordinate, ChunkSize, -ChunkSize, -ChunkSize));
        else if (blockCoordinate.x == ChunkSize - 1 && blockCoordinate.y == 0 && blockCoordinate.z == ChunkSize - 1)
            MapLoad.RefreshExistingChunk(Utility.AddToVector(chunkCoordinate, ChunkSize, -ChunkSize, ChunkSize));
        else if (blockCoordinate.x == ChunkSize - 1 && blockCoordinate.y == ChunkSize - 1 && blockCoordinate.z == 0)
            MapLoad.RefreshExistingChunk(Utility.AddToVector(chunkCoordinate, ChunkSize, ChunkSize, -ChunkSize));
        else if (blockCoordinate.x == ChunkSize - 1 && blockCoordinate.y == ChunkSize - 1 && blockCoordinate.z == ChunkSize - 1)
            MapLoad.RefreshExistingChunk(Utility.AddToVector(chunkCoordinate, ChunkSize, ChunkSize, ChunkSize));

        // Edge checks along X, Y, and Z axes
        if (blockCoordinate.x == 0 && blockCoordinate.y == 0)
            MapLoad.RefreshExistingChunk(Utility.AddToVector(chunkCoordinate, -ChunkSize, -ChunkSize, 0));
        else if (blockCoordinate.x == 0 && blockCoordinate.z == 0)
            MapLoad.RefreshExistingChunk(Utility.AddToVector(chunkCoordinate, -ChunkSize, 0, -ChunkSize));
        else if (blockCoordinate.y == 0 && blockCoordinate.z == 0)
            MapLoad.RefreshExistingChunk(Utility.AddToVector(chunkCoordinate, 0, -ChunkSize, -ChunkSize));
        else if (blockCoordinate.x == ChunkSize - 1 && blockCoordinate.y == 0)
            MapLoad.RefreshExistingChunk(Utility.AddToVector(chunkCoordinate, ChunkSize, -ChunkSize, 0));
        else if (blockCoordinate.x == ChunkSize - 1 && blockCoordinate.z == 0)
            MapLoad.RefreshExistingChunk(Utility.AddToVector(chunkCoordinate, ChunkSize, 0, -ChunkSize));
        else if (blockCoordinate.y == ChunkSize - 1 && blockCoordinate.z == 0)
            MapLoad.RefreshExistingChunk(Utility.AddToVector(chunkCoordinate, 0, ChunkSize, -ChunkSize));
        else if (blockCoordinate.x == 0 && blockCoordinate.y == ChunkSize - 1)
            MapLoad.RefreshExistingChunk(Utility.AddToVector(chunkCoordinate, -ChunkSize, ChunkSize, 0));
        else if (blockCoordinate.x == 0 && blockCoordinate.z == ChunkSize - 1)
            MapLoad.RefreshExistingChunk(Utility.AddToVector(chunkCoordinate, -ChunkSize, 0, ChunkSize));
        else if (blockCoordinate.y == 0 && blockCoordinate.z == ChunkSize - 1)
            MapLoad.RefreshExistingChunk(Utility.AddToVector(chunkCoordinate, 0, -ChunkSize, ChunkSize));
        else if (blockCoordinate.x == ChunkSize - 1 && blockCoordinate.y == ChunkSize - 1)
            MapLoad.RefreshExistingChunk(Utility.AddToVector(chunkCoordinate, ChunkSize, ChunkSize, 0));
        else if (blockCoordinate.x == ChunkSize - 1 && blockCoordinate.z == ChunkSize - 1)
            MapLoad.RefreshExistingChunk(Utility.AddToVector(chunkCoordinate, ChunkSize, 0, ChunkSize));
        else if (blockCoordinate.y == ChunkSize - 1 && blockCoordinate.z == ChunkSize - 1)
            MapLoad.RefreshExistingChunk(Utility.AddToVector(chunkCoordinate, 0, ChunkSize, ChunkSize));
 
    }
 
}