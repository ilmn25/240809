using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class WorldGen
{
    protected static System.Random Random;
    public static readonly Vector3Int Size = new Vector3Int(15, 5, 15);
    // public static readonly Vector3Int Size = new Vector3Int(5, 2, 5);
    public static readonly Vector3Int SpawnPoint = 
        new (World.ChunkSize * 2, World.ChunkSize * (Size.y - 1), World.ChunkSize * 2); 
 
    protected static readonly bool Flat = false; 
    
    protected static Chunk CurrentChunk;
    protected static Vector3Int CurrentCoordinate; 
    
    protected static readonly int WorldHeight = (Size.y - 2) * World.ChunkSize;
    
    public static float GetOffset()
    {
        return (float)Random.NextDouble() * 1000;
    }
    public static void Initialize() {
        Random = new System.Random(World.Seed);
    }

    public static void GenerateTestMap()
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
        
        player = (PlayerInfo) Entity.CreateInfo(ID.Player, playerSpawnPoint);
        player.CharSprite = ID.Yuuri;
        World.Inst[SpawnPoint].DynamicEntity.Add(player); 
        World.Inst.target.Add(player);

        // player = (PlayerInfo) Entity.CreateInfo(ID.Player, playerPos);
        // player.CharSprite = ID.Yuuri;
        // World.Inst[playerPos].DynamicEntity.Add(player); 
        // World.Inst.target.Add(player);
        if (Flat) return;
        // int chunkSize = World.ChunkSize;
        // for (int x = 0; x < World.Inst.Bounds.x; x++)
        // {
        //     for (int y = 0; y < World.Inst.Bounds.y - 1; y++)
        //     {
        //         for (int z = 0; z < World.Inst.Bounds.z; z++)
        //         { 
        //             Vector3Int worldPos = new Vector3Int(x, y, z);
        //             Vector3Int chunkPos = NavMap.GetRelativePosition(worldPos);
        //             int localChunkX = worldPos.x % chunkSize;
        //             int localChunkY = worldPos.y % chunkSize;
        //             int localChunkZ = worldPos.z % chunkSize;
        //
        //             if (World.Inst[chunkPos.x, chunkPos.y, chunkPos.z][localChunkX, localChunkY, localChunkZ] == Block.ConvertID(ID.DirtBlock) &&
        //                 localChunkY + 1 != chunkSize &&
        //                 World.Inst[chunkPos.x, chunkPos.y, chunkPos.z][localChunkX, localChunkY + 1, localChunkZ] == 0)
        //             {
        //                 if (Random.NextDouble() <= 0.0004)
        //                 {
        //                     global::SetPiece.PasteSetPiece(new Vector3Int(x, y+1, z), global::SetPiece.LoadSetPieceFile("house_stone"));
        //                 }
        //                 // else if (Random.NextDouble() <= 0.001)
        //                 // {
        //                 //     global::SetPiece.PasteSetPiece(new Vector3Int(x, y, z), global::SetPiece.LoadSetPieceFile("tree_a"));
        //                 // }
        //                 // else if (Random.NextDouble() <= 0.001)
        //                 // {
        //                 //     global::SetPiece.PasteSetPiece(new Vector3Int(x, y, z), global::SetPiece.LoadSetPieceFile("tree_b")); 
        //                 // } 
        //             } 
        //         }
        //     }
        // }
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
                        NavMap.SetChunk(position);
                        yield return null;  
                    }
                }
            }
        }
    }

    public static void Generate(Vector3Int coordinates)
    {
        CurrentCoordinate = coordinates;
        CurrentChunk = new Chunk();
        World.Inst[coordinates] = CurrentChunk;

        if (!Flat)
        {
            // Stopwatch stopwatch = new Stopwatch();
            // stopwatch.Start();
            GenTaskStone.Run();
            GenTaskGranite.Run();  
            GenTaskMarble.Run();
            GenTaskDirt.Run();
            GenTaskSand.Run();  
            GenTaskMaze.Run();
            GenTaskCrater.Run(); 
            GenTaskCaves.Run();    
            GenTaskEntity.Run();    
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
