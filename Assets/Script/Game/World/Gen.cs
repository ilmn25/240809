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
    
    protected static System.Random Random = new (SaveData.Inst?.seed ?? 0);
    protected static Gen _target;

    public static readonly Dictionary<GenType, Gen> Dictionary = new ()
    {
        {GenType.Abyss, new GenAbyss()},
        {GenType.SkyBlock, new GenSkyBlock()},
        {GenType.SuperFlat, new GenSuperFlat()},
    };
    public static void Initialize(GenType genType)
    {
        World.Inst = SaveData.Inst.worlds[genType];
        Random = new System.Random(SaveData.Inst.seed);
        _target = Dictionary[genType];
        _target.WorldHeight = (World.Inst.Size.y - 3) * World.ChunkSize;
    }
    public static float GetOffset()
    {
        return (float)Random.NextDouble() * 1000f;
    }

    public static IEnumerator GenerateNearbyChunks(Vector3Int center)
    {
        Vector3Int position;
        for (int x = -Scene.GenRange; x <= Scene.GenRange; x++)
        {
            for (int y = -Scene.GenRange; y <= Scene.GenRange; y++)
            {
                for (int z = -Scene.GenRange; z <= Scene.GenRange; z++)
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
