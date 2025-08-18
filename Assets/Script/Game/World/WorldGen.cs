using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class WorldGen
{
    protected static System.Random Random;

    public static float GetOffset()
    {
        return (float)Random.NextDouble() * 1000;
    }

    protected static readonly bool SpawnStaticEntity = true;
    protected static readonly bool SpawnDynamicEntity = true;
    protected static readonly bool Flat = false;
    public static readonly Vector3Int Size = new Vector3Int(15, 5, 15);
    // private static readonly bool SpawnStaticEntity = false;
    // private static readonly bool SpawnDynamicEntity = false;
    // private static readonly bool Flat = true;
    // public static readonly Vector3Int Size = new Vector3Int(5, 2, 5);
     
    protected const int WallHeight = 5;
    protected const int FloorHeight = 2;
  
    
    protected static Chunk CurrentChunk;
    protected static Vector3Int CurrentCoordinate; 
    protected static Chunk SetPiece; 
    
    protected static readonly int WorldHeight = (Size.y - 2) * World.ChunkSize;
    
    public static void Initialize() {
        Random = new System.Random(World.Seed);
 
    }

    public static void GenerateTestMap()
    { 
        World.Inst = new World(Size.x, Size.y, Size.z);
        List<Vector3Int> coordinatesList = new List<Vector3Int>();

        for (int x = 0; x < Size.x * World.ChunkSize; x += World.ChunkSize)
        {
            for (int y = 0; y < Size.y * World.ChunkSize; y += World.ChunkSize)
            {
                for (int z = 0; z < Size.z * World.ChunkSize; z += World.ChunkSize)
                {
                    coordinatesList.Add(new Vector3Int(x, y, z));
                }
            }
        } 
        
        foreach (var coordinates in coordinatesList)
        {
            World.Inst[coordinates] = Generate(coordinates);
        }

        Vector3Int playerPos = new Vector3Int(World.ChunkSize * Size.x / 2, World.ChunkSize * Size.y - 5, World.ChunkSize * Size.z / 2);
        PlayerInfo player = (PlayerInfo) Entity.CreateInfo("player", playerPos);
        World.Inst[playerPos].DynamicEntity.Add(player); 
        World.Inst.target.Add(player);
        
        player = (PlayerInfo) Entity.CreateInfo("player", playerPos);
        World.Inst[playerPos].DynamicEntity.Add(player); 
        World.Inst.target.Add(player);

        player = (PlayerInfo) Entity.CreateInfo("player", playerPos);
        World.Inst[playerPos].DynamicEntity.Add(player); 
        World.Inst.target.Add(player);
        if (Flat) return;
        int chunkSize = World.ChunkSize;
        for (int x = 0; x < World.Inst.Bounds.x; x++)
        {
            for (int y = 0; y < World.Inst.Bounds.y - 1; y++)
            {
                for (int z = 0; z < World.Inst.Bounds.z; z++)
                { 
                    Vector3Int worldPos = new Vector3Int(x, y, z);
                    Vector3Int chunkPos = NavMap.GetRelativePosition(worldPos);
                    int localChunkX = worldPos.x % chunkSize;
                    int localChunkY = worldPos.y % chunkSize;
                    int localChunkZ = worldPos.z % chunkSize;
        
                    if (World.Inst[chunkPos.x, chunkPos.y, chunkPos.z][localChunkX, localChunkY, localChunkZ] == Block.ConvertID("dirt") &&
                        localChunkY + 1 != chunkSize &&
                        World.Inst[chunkPos.x, chunkPos.y, chunkPos.z][localChunkX, localChunkY + 1, localChunkZ] == 0)
                    {
                        if (Random.NextDouble() <= 0.0004)
                        {
                            global::SetPiece.PasteSetPiece(new Vector3Int(x, y+1, z), global::SetPiece.LoadSetPieceFile("house_stone"));
                        }
                        // else if (Random.NextDouble() <= 0.001)
                        // {
                        //     global::SetPiece.PasteSetPiece(new Vector3Int(x, y, z), global::SetPiece.LoadSetPieceFile("tree_a"));
                        // }
                        // else if (Random.NextDouble() <= 0.001)
                        // {
                        //     global::SetPiece.PasteSetPiece(new Vector3Int(x, y, z), global::SetPiece.LoadSetPieceFile("tree_b")); 
                        // } 
                    } 
                }
            }
        }
    }

    private static Chunk Generate(Vector3Int coordinates)
    {
        CurrentCoordinate = coordinates;
        CurrentChunk = new Chunk();

        if (Flat)
        {
            if (coordinates.y == 0)
            {
                for (int z = 0; z < World.ChunkSize; z++)
                {
                    for (int x = 0; x < World.ChunkSize; x++)
                    {
                        CurrentChunk[x, 0, z] = 1;
                    }
                }
            }
            return CurrentChunk;
        }
        
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
        // HandleBlockGeneration();
        HandleEntityGeneration(CurrentChunk, CurrentCoordinate);

        // stopwatch.Stop();
        // Debug.Log($"Generation completed in {stopwatch.ElapsedMilliseconds} ms");
        
        return CurrentChunk;
    }   
  
 
    private static void HandleEntityGeneration(Chunk chunk, Vector3Int coordinates)
    { 
        
        for (int x = 0; x < World.ChunkSize; x++)
        {
            for (int y = 0; y < World.ChunkSize; y++)
            {
                for (int z = 0; z <  World.ChunkSize; z++)
                {
                    if (
                        y + 1 < World.ChunkSize &&
                        chunk[x, y, z] != 0 && 
                        chunk[x, y + 1, z] == 0) 
                    {
                        if (SpawnStaticEntity)
                        {
                            if (chunk[x, y, z] == Block.ConvertID("dirt"))
                            {
                                {
                                    double rng = Random.NextDouble();
                                    if (rng <= 0.02)
                                    {
                                        chunk.StaticEntity.Add(Entity.CreateInfo("tree", coordinates + new Vector3(x, y + 1, z)));
                                    }
                                    else if (rng <= 0.04)
                                    {
                                        chunk.StaticEntity.Add(Entity.CreateInfo("bush1", coordinates + new Vector3(x, y + 1, z)));
                                    }
                                    else if (rng <= 0.2)
                                    {
                                        chunk.StaticEntity.Add(Entity.CreateInfo("grass", coordinates + new Vector3(x, y + 1, z)));
                                    }
                                }
                            }
                            else
                            {
                                if (Random.NextDouble() <= 0.0004)
                                {
                                    chunk.StaticEntity.Add(Entity.CreateInfo("chest", coordinates + new Vector3(x, y + 1, z)));
                                } 
                                else if (Random.NextDouble() <= 0.02)
                                {
                                    chunk.StaticEntity.Add(Entity.CreateInfo("slab", coordinates + new Vector3(x, y + 1, z)));
                                }
                            }
 
                        }
                        if (SpawnDynamicEntity && Random.NextDouble() <= 0.002)
                        { 
                            if (Random.NextDouble() <= 0.5)
                            {
                                chunk.DynamicEntity.Add(Entity.CreateInfo("flint", coordinates + new Vector3Int(x, y + 1, z)));
                            } else
                            {
                                chunk.DynamicEntity.Add(Entity.CreateInfo("sticks", coordinates + new Vector3Int(x, y + 1, z))); 
                            }
                        }
                    } 
                }
            }
        }
    }
     
 
}
