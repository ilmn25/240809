using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class WorldGen
{
    protected static System.Random Random = new (World.Seed);
    public static Vector3Int Size;
    protected static int WorldHeight;
    public static Vector3Int SpawnPoint;
 
    protected static readonly bool Flat = false;  

    public static void SetSize(Vector3Int size)
    {
        Size = size;
        WorldHeight = (Size.y - 3) * World.ChunkSize;
        SpawnPoint = 
            new (Size.x * (World.ChunkSize - 2), 
                World.ChunkSize * (Size.y - 1),
                Size.z * (World.ChunkSize - 2)); 
    }
    public static float GetOffset()
    {
        return (float)Random.NextDouble() * 1000;
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
        
        // while (!NavMap.Get(playerSpawnPoint)) playerSpawnPoint.y++; 
        
        PlayerInfo player = (PlayerInfo) Entity.CreateInfo(ID.Player, SpawnPoint);
        player.spawnPoint = SpawnPoint;
        World.Inst[SpawnPoint].DynamicEntity.Add(player); 
        World.Inst.target.Add(player);
        
        if(Game.BuildMode)return;
        player = (PlayerInfo) Entity.CreateInfo(ID.Player, SpawnPoint);
        player.spawnPoint = SpawnPoint;
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
