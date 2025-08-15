using System.Collections;
using Unity.Mathematics;
using UnityEngine;
 
public class NavMap
{
    private static BitArray _bitMap;
    private static Vector3Int _bounds; 
    private static int GetIndex(int x, int y, int z)
    {
        return x + _bounds.x * (y + _bounds.y * z);
    }
    private static int GetIndex(Vector3Int coordinate)
    {
        return coordinate.x + _bounds.x * (coordinate.y + _bounds.y * coordinate.z);
    }
    public static void GenerateNavMap()
    {
        Chunk chunk;
        _bounds = World.Inst.Bounds;
        _bitMap = new BitArray(_bounds.x * _bounds.y * _bounds.z); 
        
        for (int wx = 0; wx < World.Inst.Length.x; wx++)
        { 
            for (int wy = 0; wy < World.Inst.Length.y; wy++)
            {
                for (int wz = 0; wz < World.Inst.Length.z; wz++)
                {
                    int chunkX = wx * World.ChunkSize;
                    int chunkY = wy * World.ChunkSize;
                    int chunkZ = wz * World.ChunkSize;
                    chunk = World.Inst[chunkX, chunkY, chunkZ]; 
                    if (chunk != null)
                    { 
                        for (int i = 0; i < World.ChunkSize; i++)
                        {
                            for (int j = 0; j < World.ChunkSize; j++)
                            {
                                for (int k = 0; k < World.ChunkSize; k++)
                                {
                                    Set(chunkX + i, chunkY + j, chunkZ + k, chunk[i, j, k] == 0);
                                }
                            }
                        }
                        foreach (var entity in chunk.StaticEntity)
                        {
                            int entityX = Mathf.FloorToInt(entity.position.x);
                            int entityY = Mathf.FloorToInt(entity.position.y);
                            int entityZ = Mathf.FloorToInt(entity.position.z);

                            Vector3Int bounds = Entity.Dictionary[entity.stringID].Bounds;
                            int entityEndX = entityX + bounds.x;
                            int entityEndY = entityY + bounds.y;
                            int entityEndZ = entityZ + bounds.z;

                            for (int x = entityX; x < entityEndX; x++)
                            {
                                for (int y = entityY; y < entityEndY; y++)
                                {
                                    for (int z = entityZ; z < entityEndZ; z++)
                                    {
                                        Set(x, y, z, false);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        } 
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

    public static void Set(Vector3Int worldPosition, bool value, bool check = false)
    {
        if (check && !World.IsInWorldBounds(worldPosition)) return;
        _bitMap[GetIndex(worldPosition)] = value;
    }
    public static void Set(int x, int y, int z, bool value, bool check = false)
    {
        if (check && !World.IsInWorldBounds(x, y, z)) return;
        _bitMap[GetIndex(x, y, z)] = value;
    }
} 