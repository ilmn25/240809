using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;
using Debug = UnityEngine.Debug;

[Serializable]
public enum GenType
{
    Abyss, SkyBlock, SuperFlat
}
public class Gen
{
    public int WorldHeight { get; protected set; }
    protected virtual void GenChunk(Vector3Int currentCoordinate, Chunk currentChunk) { }
    public virtual Vector3Int GetSize() => Vector3Int.one;
    public virtual Vector3Int GetSpawnPoint() => Vector3Int.zero;
    
    protected static System.Random Random = new (Save.Inst?.seed ?? 0);
    protected static Gen _target;

    public static readonly Dictionary<GenType, Gen> Dictionary = new ()
    {
        {GenType.Abyss, new GenAbyss()},
        {GenType.SkyBlock, new GenSkyBlock()},
        {GenType.SuperFlat, new GenSuperFlat()},
    };
    public static void Initialize(GenType genType)
    {
        Random = new System.Random(Save.Inst.seed);
        _target = Dictionary[genType];
        _target.WorldHeight = (World.Inst.Size.y - 3) * World.ChunkSize;
    }
    public static float GetOffset()
    {
        return (float)Random.NextDouble() * 1000f;
    }

    public static IEnumerator GenerateNearbyChunks(Vector3Int center, int range)
    {
        Vector3Int position;
        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                for (int z = -range; z <= range; z++)
                {
                    position = new Vector3Int(
                        center.x + x * World.ChunkSize,
                        center.y + y * World.ChunkSize,
                        center.z + z * World.ChunkSize);

                    if (World.Inst[position] == null)
                    {
                        Generate(position);
                        yield return null;  
                    }
                }
            }
        }
    }

    public static void Generate(Vector3Int currentCoordinate)
    {
        Chunk currentChunk = new Chunk();
        World.Inst[currentCoordinate] = currentChunk;
        _target.GenChunk(currentCoordinate, currentChunk);
    }   
}
