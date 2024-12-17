using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class WorldGen
{
    private static readonly System.Random Random = new System.Random();

    public const bool SpawnStaticEntity = true;
    public const bool SpawnDynamicEntity = false;
    public const bool Flat = false;
    public static readonly Vector3Int Size = new Vector3Int(30, 7, 30);

    private static float StoneOffsetX;
    private static float StoneOffsetZ;
    private static float DirtOffsetX;
    private static float DirtOffsetZ;
    private static float SandOffsetX;
    private static float SandOffsetZ;
    private static float MarbleOffsetX;
    private static float MarbleOffsetZ;
    private static float CaveOffset;

    private const float StoneScale = 0.01f;
    private const float DirtScale = 0.005f;
    private const float SandScale = 0.02f;
    private const float MarbleScale = 0.007f;
    private const float CaveScale = 0.06f;
    private const int WallHeight = 5;
    private const int FloorHeight = 2;

    private static ChunkData _chunkData;
    private static Vector3Int _chunkCoord; 
    private static ChunkData _setPiece; 
    private static readonly int _worldHeight = (Size.y - 2) * World.ChunkSize;
    
    public static void Initialize() {
        StoneOffsetX = UnityEngine.Random.Range(0f, 1000f);
        StoneOffsetZ = UnityEngine.Random.Range(0f, 1000f);
        DirtOffsetX = UnityEngine.Random.Range(0f, 1000f);
        DirtOffsetZ = UnityEngine.Random.Range(0f, 1000f);
        SandOffsetX = UnityEngine.Random.Range(0f, 1000f);
        SandOffsetZ = UnityEngine.Random.Range(0f, 1000f);
        MarbleOffsetX = UnityEngine.Random.Range(0f, 1000f);
        MarbleOffsetZ = UnityEngine.Random.Range(0f, 1000f);
        CaveOffset = UnityEngine.Random.Range(0f, 1000f); 
    }


    //! debug tools
    public static void Generate()
    { 
        World.world = new World(Size.x, Size.y, Size.z);
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
            World.world[coordinates.x, coordinates.y, coordinates.z] = GenerateTestChunk(coordinates); 
        }

        int chunkSize = World.ChunkSize;
        for (int x = 0; x < World.world.Bounds.x; x++)
        {
            for (int y = 0; y < World.world.Bounds.y - 1; y++)
            {
                for (int z = 0; z < World.world.Bounds.z; z++)
                { 
                    Vector3Int worldPos = new Vector3Int(x, y, z);
                    Vector3Int chunkPos = NavMap.GetRelativePosition(worldPos);
                    int localChunkX = worldPos.x % chunkSize;
                    int localChunkY = worldPos.y % chunkSize;
                    int localChunkZ = worldPos.z % chunkSize;

                    if (World.world[chunkPos.x, chunkPos.y, chunkPos.z].Map[localChunkX, localChunkY, localChunkZ] == Block.ConvertID("dirt") &&
                        localChunkY + 1 != chunkSize &&
                        World.world[chunkPos.x, chunkPos.y, chunkPos.z].Map[localChunkX, localChunkY + 1, localChunkZ] == 0)
                    {
                        if (Random.NextDouble() <= 0.0002)
                        {
                            SetPiece.PasteSetPiece(new Vector3Int(x, y+2, z), SetPiece.LoadSetPieceFile("house_stone"));
                        }
                        else if (Random.NextDouble() <= 0.002)
                        {
                            SetPiece.PasteSetPiece(new Vector3Int(x, y+1, z), SetPiece.LoadSetPieceFile("tree_a"));
                        }
                        else if (Random.NextDouble() <= 0.002)
                        {
                            SetPiece.PasteSetPiece(new Vector3Int(x, y+1, z), SetPiece.LoadSetPieceFile("tree_b")); 
                        } 
                    } 
                }
            }
        }
 
    }  

    public static ChunkData GenerateTestChunk(Vector3Int coordinates)
    {
        _chunkCoord = coordinates;
        _chunkData = new ChunkData();

        if (Flat)
        {
            if (coordinates.y == 0)
            {
                for (int z = 0; z < World.ChunkSize; z++)
                {
                    for (int x = 0; x < World.ChunkSize; x++)
                    {
                        _chunkData.Map[x, 0, z] = 1;
                    }
                }
            }
        }
        else
        {
            HandleBlockGeneration();
            HandleEntityGeneration(_chunkData); 
        } 
        
        return _chunkData;
    }  
    
    private static void HandleBlockGeneration()
    {
        bool wall = new System.Random().NextDouble() < 0.1;
        bool[,] maze = HandleMazeAlgorithm(World.ChunkSize, World.ChunkSize);

        // Calculate the center of the map
        int centerX = World.world.Bounds.x / 3;
        int centerZ = World.world.Bounds.z / 3;
        int craterRadius = 35; // Adjust the radius as needed
    
        for (int y = 0; y < World.ChunkSize; y++)
        {
            for (int x = 0; x < World.ChunkSize; x++)
            {
                for (int z = 0; z < World.ChunkSize; z++)
                {
                    if (IsWithinCrater(x, y, z, centerX, centerZ, craterRadius))
                    {
                        _chunkData.Map[x, y, z] = 0; // Empty space for the crater
                        continue;
                    }

                    GenerateTerrainBlocks(x, y, z);
                    GenerateCaves(x, y, z);
                    HandleBackroomGeneration(x, y, z, maze);
                }
            }
        }
    }
    private static bool IsWithinCrater(int x, int y, int z, int centerX, int centerZ, int craterRadius)
    {
        int distanceX = Mathf.Abs(centerX - (_chunkCoord.x + x));
        int distanceZ = Mathf.Abs(centerZ - (_chunkCoord.z + z));
        float distanceFromCenter = Mathf.Sqrt(distanceX * distanceX + distanceZ * distanceZ);

        // Calculate the taper factor so that it decreases with depth
        float taperFactor = (float)(y + _chunkCoord.y) / _worldHeight;
        float taperedRadius = craterRadius * taperFactor;

        // Add noise to the crater walls
        float noiseX = (_chunkCoord.x + x) * CaveScale + CaveOffset;
        float noiseZ = (_chunkCoord.z + z) * CaveScale + CaveOffset;
        float wallNoise = Mathf.PerlinNoise(noiseX, noiseZ) * 2.0f - 1.0f; // Noise value between -1 and 1
        taperedRadius += wallNoise;

        return distanceFromCenter <= taperedRadius;
    }
    
    private static void GenerateTerrainBlocks(int x, int y, int z)
    {
        float stoneX = Mathf.Abs(_chunkCoord.x + x) * StoneScale + StoneOffsetX;
        float stoneZ = Mathf.Abs(_chunkCoord.z + z) * StoneScale + StoneOffsetZ;
        float stoneNoiseValue = Mathf.PerlinNoise(stoneX, stoneZ);
        int stoneHeight = Mathf.FloorToInt(stoneNoiseValue * (_worldHeight / 4)) + (_worldHeight * 3 / 4);

        float dirtX = Mathf.Abs(_chunkCoord.x + x) * DirtScale + DirtOffsetX;
        float dirtZ = Mathf.Abs(_chunkCoord.z + z) * DirtScale + DirtOffsetZ;  
        float dirtNoiseValue = Mathf.PerlinNoise(dirtX, dirtZ);
        int dirtHeight = Mathf.FloorToInt(dirtNoiseValue * (_worldHeight / 4)) + (_worldHeight * 3 / 4);

        float sandX = Mathf.Abs(_chunkCoord.x + x) * SandScale + SandOffsetX;
        float sandZ = Mathf.Abs(_chunkCoord.z + z) * SandScale + SandOffsetZ;
        float sandNoiseValue = Mathf.PerlinNoise(sandX, sandZ);
        int sandHeight = Mathf.FloorToInt(sandNoiseValue * (_worldHeight / 4)) + (_worldHeight * 3 / 4);

        float marbleX = Mathf.Abs(_chunkCoord.x + x) * MarbleScale + MarbleOffsetX;
        float marbleZ = Mathf.Abs(_chunkCoord.z + z) * MarbleScale + MarbleOffsetZ;
        float marbleNoiseValue = Mathf.PerlinNoise(marbleX, marbleZ);
        int marbleHeight = Mathf.FloorToInt(marbleNoiseValue * (_worldHeight / 4)) + (_worldHeight * 3 / 4);

        if (y + _chunkCoord.y > WallHeight + FloorHeight)
        {
            if (y + _chunkCoord.y <= marbleHeight - 50)
            {
                _chunkData.Map[x, y, z] = Block.ConvertID("marble");
            } 
            else if (y + _chunkCoord.y <= stoneHeight - 5)
            {
                _chunkData.Map[x, y, z] = Block.ConvertID("stone");
            }
            else if (y + _chunkCoord.y <= dirtHeight)
            {
                _chunkData.Map[x, y, z] = Block.ConvertID("dirt");
            }
            else if (y + _chunkCoord.y <= sandHeight)
            {
                _chunkData.Map[x, y, z] = Block.ConvertID("sand");
            }
        }
    }

    private static void GenerateCaves(int x, int y, int z)
    {
        float caveX = (_chunkCoord.x + x) * CaveScale + CaveOffset;
        float caveY = (_chunkCoord.y + y) * CaveScale + CaveOffset;
        float caveZ = (_chunkCoord.z + z) * CaveScale + CaveOffset;
        float caveNoiseValue = Mathf.PerlinNoise(caveX, caveY) * Mathf.PerlinNoise(caveY, caveZ);

        if (caveNoiseValue > 0.35f)
        {
            _chunkData.Map[x, y, z] = 0; // Empty space for caves
        }
    }
 
    private static void HandleBackroomGeneration(int x, int y, int z, bool[,] maze)
    {
        if (y + _chunkCoord.y < FloorHeight)
        {
            _chunkData.Map[x, y, z] = Block.ConvertID("backroom"); // Floor
        }
        else if (y + _chunkCoord.y == WallHeight + FloorHeight)
        {
            _chunkData.Map[x, y, z] = Block.ConvertID("backroom"); // Ceiling
        }
        else if (maze[x, z] && y + _chunkCoord.y < WallHeight + FloorHeight)
        {
            _chunkData.Map[x, y, z] = Block.ConvertID("backroom"); // Walls
        }
    }
 
    private static void HandleEntityGeneration(ChunkData chunkData)
    { 
        ChunkEntityData entityData;
        SerializableVector3Int entityPosition;
        
        for (int x = 0; x < World.ChunkSize; x++)
        {
            for (int y = 0; y < World.ChunkSize; y++)
            {
                for (int z = 0; z <  World.ChunkSize; z++)
                {
                    if (
                        y + 1 < World.ChunkSize &&
                        chunkData.Map[x, y, z] != 0 && 
                        chunkData.Map[x, y + 1, z] == 0) 
                    {
                        if (SpawnStaticEntity)
                        {
                            if (chunkData.Map[x, y, z] == Block.ConvertID("dirt"))
                            {
                                {
                                    double rng = Random.NextDouble();
                                    if (rng <= 0.03)
                                    {
                                        entityData = Entity.GetChunkEntityData("tree", new SerializableVector3Int(x, y + 1, z));
                                        chunkData.StaticEntity.Add(entityData);
                                    }
                                    else if (rng <= 0.03)
                                    {
                                        entityData = Entity.GetChunkEntityData("bush1", new SerializableVector3Int(x, y + 1, z));
                                        chunkData.StaticEntity.Add(entityData);
                                    }
                                    else if (rng <= 0.2)
                                    {
                                        entityData = Entity.GetChunkEntityData("grass", new SerializableVector3Int(x, y + 1, z));
                                        chunkData.StaticEntity.Add(entityData);
                                    }
                                }
                            }
                            else
                            {
                                if (Random.NextDouble() <= 0.003)
                                {

                                    entityData = Entity.GetChunkEntityData("stage_hand", new SerializableVector3Int(x, y + 1, z));
                                    chunkData.StaticEntity.Add(entityData);
                                }
                                else if (Random.NextDouble() <= 0.03)
                                {
                                    entityData = Entity.GetChunkEntityData("slab", new SerializableVector3Int(x, y + 1, z));
                                    chunkData.StaticEntity.Add(entityData);
                                }
                            }
 
                        }
                        if (SpawnDynamicEntity && Random.NextDouble() <= 0.003)
                        {
                            entityPosition = new SerializableVector3Int(x, y + 1, z);
                            if (Random.NextDouble() <= 0.5)
                            {
                                if (Random.NextDouble() <= 0.5)
                                {
                                    entityData = Entity.GetChunkEntityData("snare_flea", entityPosition);
                                } else
                                {
                                    entityData = Entity.GetChunkEntityData("chito", entityPosition);
                                }
                            } else { 
                                if (Random.NextDouble() <= 0.5)
                                {
                                    entityData = Entity.GetChunkEntityData("megumin", entityPosition);
                                } else
                                {
                                    entityData = Entity.GetChunkEntityData("yuuri", entityPosition);
                                }
                            }
                            chunkData.DynamicEntity.Add(entityData); 
                        }
                    } 
                }
            }
        }
    }
    
    private static bool[,] HandleMazeAlgorithm(int width, int height)
    {
        bool[,] maze = new bool[width, height];
        System.Random rand = new System.Random();

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                // Create walls with one block thickness and corridors with four blocks width
                maze[x, z] = (x % 5 == 0 || z % 5 == 0);
            }
        }

        // Randomly remove entire wall sections
        for (int x = 0; x < width; x += 8)
        {
            for (int z = 0; z < height; z += 8)
            {
                if (rand.NextDouble() < 0.8) // 30% chance to remove a wall section
                {
                    // Remove vertical wall section
                    for (int i = 0; i < 8 && x + i < width; i++)
                    {
                        maze[x + i, z] = false;
                    }
                    // Remove horizontal wall section
                    for (int j = 0; j < 8 && z + j < height; j++)
                    {
                        maze[x, z + j] = false;
                    }
                }
            }
        }

        return maze;
    }
 
}
