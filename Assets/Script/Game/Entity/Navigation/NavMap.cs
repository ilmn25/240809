using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
 
public class NavMap
{
    private static BitArray _bitMap;
    private static readonly List<Vector3Int> LoadedChunks = new ();

    public static IReadOnlyList<Vector3Int> GetLoadedChunks() => LoadedChunks;

    public static void Initialize()
    {
        _bitMap = new (
            World.Inst.Bounds.x * World.Inst.Bounds.y * World.Inst.Bounds.z);
        LoadedChunks.Clear();
    }
    
    private static int GetIndex(int x, int y, int z)
    {
        return x + World.Inst.Bounds.x * (y + World.Inst.Bounds.y * z);
    }
    private static int GetIndex(Vector3Int coordinate)
    {
        return coordinate.x + World.Inst.Bounds.x * (coordinate.y + World.Inst.Bounds.y * coordinate.z);
    } 
    /// <summary>Process a chunk into NavMap. Returns true if new data was loaded (false if already done).</summary>
    public static bool SetChunk(Vector3Int coordinate)
    { 
        if (!World.IsInWorldBounds(coordinate) || LoadedChunks.Contains(coordinate)) return false;
        LoadedChunks.Add(coordinate);
        Chunk chunk = World.Inst[coordinate.x, coordinate.y, coordinate.z]; 
        if (chunk != null)
        { 
            for (int x = 0; x < World.ChunkSize; x++)
            {
                for (int y = 0; y < World.ChunkSize; y++)
                {
                    for (int z = 0; z < World.ChunkSize; z++)
                    {
                        Set(coordinate.x + x, coordinate.y + y, coordinate.z + z, chunk[x, y, z] == 0);
                    }
                }
            }
            foreach (var entity in chunk.StaticEntity)
            {
                SetEntity(Entity.Dictionary[entity.id], entity.position, false);
            }
        }
        return true;
    }

    public static Vector3Int GetRelativePosition(Vector3Int coordinate)
    {
        return coordinate;
    }

    public static bool Get(Vector3Int worldPosition)
    {
        if (!World.IsInWorldBounds(worldPosition)) return true;
        return _bitMap[GetIndex(worldPosition)];
    }

    public static void Set(Vector3Int worldPosition, bool value, bool isAir = false)
    {
        if (isAir && !World.IsInWorldBounds(worldPosition)) return;
        _bitMap[GetIndex(worldPosition)] = value; 
    }
    
    public static void Set(int x, int y, int z, bool value, bool isAir = false)
    {
        if (isAir && !World.IsInWorldBounds(x, y, z)) return;
        _bitMap[GetIndex(x, y, z)] = value;
    }

    /// <summary>
    /// Pack a chunk's NavMap bits (air=true) into a byte array for network transfer.
    /// ChunkSize³ bits → ceil(ChunkSize³ / 8) bytes.
    /// </summary>
    public static byte[] PackChunk(Vector3Int chunkCoord)
    {
        int bitsPerChunk = World.ChunkSize * World.ChunkSize * World.ChunkSize;
        int byteCount = (bitsPerChunk + 7) / 8;
        byte[] data = new byte[byteCount];

        int bitIndex = 0;
        for (int x = 0; x < World.ChunkSize; x++)
        {
            for (int y = 0; y < World.ChunkSize; y++)
            {
                for (int z = 0; z < World.ChunkSize; z++)
                {
                    int worldX = chunkCoord.x + x;
                    int worldY = chunkCoord.y + y;
                    int worldZ = chunkCoord.z + z;
                    if (_bitMap[GetIndex(worldX, worldY, worldZ)])
                        data[bitIndex >> 3] |= (byte)(1 << (bitIndex & 7));
                    bitIndex++;
                }
            }
        }
        return data;
    }

    /// <summary>
    /// Apply NavMap chunk data received from the host.  Marks the chunk as loaded
    /// so local SetChunk calls (which would re-read from possibly-stale chunk data) are skipped.
    /// </summary>
    public static void ApplySyncData(Vector3Int chunkCoord, byte[] data)
    {
        if (_bitMap == null) return; // scene not yet initialized
        if (!World.IsInWorldBounds(chunkCoord)) return;

        // Mark as loaded to prevent local SetChunk from re-processing.
        if (!LoadedChunks.Contains(chunkCoord))
            LoadedChunks.Add(chunkCoord);

        int bitIndex = 0;
        for (int x = 0; x < World.ChunkSize; x++)
        {
            for (int y = 0; y < World.ChunkSize; y++)
            {
                for (int z = 0; z < World.ChunkSize; z++)
                {
                    bool isAir = (data[bitIndex >> 3] & (1 << (bitIndex & 7))) != 0;
                    int worldX = chunkCoord.x + x;
                    int worldY = chunkCoord.y + y;
                    int worldZ = chunkCoord.z + z;
                    _bitMap[GetIndex(worldX, worldY, worldZ)] = isAir;
                    bitIndex++;
                }
            }
        }
    }

    public static void SetEntity(Entity entity, Vector3 position, bool isAir)
    {
        if (entity.Collision != Main.IndexCollide) return; 
        int entityX = Mathf.FloorToInt(position.x);
        int entityY = Mathf.FloorToInt(position.y);
        int entityZ = Mathf.FloorToInt(position.z);

        Vector3Int bounds = Vector3Int.FloorToInt(entity.Bounds);
        int entityEndX = entityX + bounds.x;
        int entityEndY = entityY + bounds.y;
        int entityEndZ = entityZ + bounds.z;

        for (int x = entityX; x < entityEndX; x++)
        {
            for (int y = entityY; y < entityEndY; y++)
            {
                for (int z = entityZ; z < entityEndZ; z++)
                {
                    NavMap.Set(x, y, z, isAir);
                }
            }
        }
    }

} 