using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class WorldGen
{
    protected static System.Random Random;
    // public static readonly Vector3Int Size = new Vector3Int(15, 6, 15);
    public static readonly Vector3Int Size = new Vector3Int(5, 4, 5);
    public static readonly Vector3Int SpawnPoint = 
        new (World.ChunkSize * 2, World.ChunkSize * (Size.y - 1), World.ChunkSize * 2); 
 
    protected static readonly bool Flat = false; 
    protected static readonly int WorldHeight = (Size.y - 2) * World.ChunkSize;
    
    public static float GetOffset()
    {
        return (float)Random.NextDouble() * 1000;
    }
    public static void Initialize() {
        Random = new System.Random(World.Seed);
    }

    public static void GenerateWorld()
    { 
        World.Inst = new World(Size.x, Size.y, Size.z); 
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
        
        Vector3Int playerSpawnPoint = SpawnPoint;
        // while (!NavMap.Get(playerSpawnPoint)) playerSpawnPoint.y++; 
        
        PlayerInfo player = (PlayerInfo) Entity.CreateInfo(ID.Player, playerSpawnPoint);
        World.Inst[SpawnPoint].DynamicEntity.Add(player); 
        World.Inst.target.Add(player);
        
        if(Game.BuildMode)return;
        player = (PlayerInfo) Entity.CreateInfo(ID.Player, playerSpawnPoint);
        player.CharSprite = ID.Yuuri;
        World.Inst[SpawnPoint].DynamicEntity.Add(player); 
        World.Inst.target.Add(player);

        // player = (PlayerInfo) Entity.CreateInfo(ID.Player, playerPos);
        // player.CharSprite = ID.Yuuri;
        // World.Inst[playerPos].DynamicEntity.Add(player); 
        // World.Inst.target.Add(player);  
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

    public static void Generate(Vector3Int coordinates)
    {
        Chunk CurrentChunk = new Chunk();
        World.Inst[coordinates] = CurrentChunk;

        if (!Flat && !Game.BuildMode)
        {
            // Stopwatch stopwatch = new Stopwatch();
            // stopwatch.Start();
            GenTaskStone.Run(coordinates, CurrentChunk);
            GenTaskGranite.Run(coordinates, CurrentChunk);  
            GenTaskMarble.Run(coordinates, CurrentChunk);
            GenTaskDirt.Run(coordinates, CurrentChunk);
            GenTaskSand.Run(coordinates, CurrentChunk);
            GenTaskMaze.Run(coordinates, CurrentChunk);
            GenTaskCrater.Run(coordinates, CurrentChunk);
            GenTaskCaves.Run(coordinates, CurrentChunk);
            GenTaskHouse.Run(coordinates, CurrentChunk);
            GenTaskThrone.Run(coordinates, CurrentChunk);
            GenTaskEntity.Run(coordinates, CurrentChunk);
            // stopwatch.Stop();
            // Debug.Log($"Generation completed in {stopwatch.ElapsedMilliseconds} ms");
            return;
        } 

        if (coordinates.y == 0)
            for (int z = 0; z < World.ChunkSize; z++)
                for (int x = 0; x < World.ChunkSize; x++)
                    CurrentChunk[x, 0, z] = 1;
    }   
}
