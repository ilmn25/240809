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
    public Vector3Int DefaultSize;
    protected virtual void GenChunk(Vector3Int currentCoordinate, Chunk currentChunk) { }
    
    protected static readonly System.Random Random = new (World.Seed);
    protected static Vector3Int SpawnPoint;
    private static Vector3Int _size; 
    protected static int WorldHeight;
    private static Gen _target;

    public static readonly Dictionary<GenType, Gen> Dictionary = new ()
    {
        {GenType.Abyss, new GenAbyss()},
        {GenType.SkyBlock, new GenSkyBlock()},
        {GenType.SuperFlat, new GenSuperFlat()},
    };
    public static void Initialize()
    {
        World.Inst = Helper.FileLoad<World>(Save.SavePath + SaveData.Inst.current);
        if (World.Inst == null)
        {
            World.Inst = new World(SaveData.Inst.current);     
            SetVariables();
            GenerateWorld();
        }
        else SetVariables();
        
        return;
        
        void SetVariables()
        {
            _target = Dictionary[SaveData.Inst.current];
            _size = World.Inst.Size;
            WorldHeight = (_size.y - 3) * World.ChunkSize;
            SpawnPoint = (_size - Vector3Int.one) * World.ChunkSize;  
        }
    }
    public static float GetOffset()
    {
        return (float)Random.NextDouble() * 1000;
    } 

    public static void GenerateWorld()
    {  
        Vector3Int position;
        for (int x = -Scene.GenRange; x <= Scene.GenRange; x++)
        {
            for (int y = -Scene.GenRange; y <= Scene.GenRange; y++)
            {
                for (int z = -Scene.GenRange; z <= Scene.GenRange; z++)
                {
                    position = new Vector3Int(
                        SpawnPoint.x + x * World.ChunkSize,
                        SpawnPoint.y + y * World.ChunkSize,
                        SpawnPoint.z + z * World.ChunkSize);

                    if (World.Inst[position] == null)
                    {
                        Generate(position);
                    }
                }
            }
        } 
         
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
