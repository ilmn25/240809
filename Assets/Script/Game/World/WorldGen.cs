using System.Collections.Generic;
using UnityEngine;

public class WorldGen
{
    private static System.Random Random;

    private static readonly bool SpawnStaticEntity = true;
    private static readonly bool SpawnDynamicEntity = true;
    private static readonly bool Flat = false;
    public static readonly Vector3Int Size = new Vector3Int(10, 4, 10);
    
    // private static readonly bool SpawnStaticEntity = false;
    // private static readonly bool SpawnDynamicEntity = false;
    // private static readonly bool Flat = true;
    // public static readonly Vector3Int Size = new Vector3Int(5, 2, 5);
    
    private const float StoneScale = 0.02f;
    private const float DirtScale = 0.05f;
    private const float SandScale = 0.05f;
    private const float MarbleScale = 0.007f;
    private const float CaveScale = 0.06f;
    private const int WallHeight = 5;
    private const int FloorHeight = 2;

    private static float _stoneOffset;
    private static float _dirtOffset;
    private static float _sandOffset;
    private static float _marbleOffset;
    private static float _caveOffset;
    
    private static Chunk _chunk;
    private static Vector3Int _chunkCoord; 
    private static Chunk _setPiece; 
    private static readonly int WorldHeight = (Size.y - 2) * World.ChunkSize;
    
    public static void Initialize() {
        Random = new System.Random(World.Seed);

        _stoneOffset = (float)Random.NextDouble() * 1000;
        _dirtOffset = (float)Random.NextDouble() * 1000;
        _sandOffset = (float)Random.NextDouble() * 1000;
        _marbleOffset = (float)Random.NextDouble() * 1000;
        _caveOffset = (float)Random.NextDouble() * 1000;
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
        Info player = Entity.CreateInfo("player", playerPos);
        World.Inst[playerPos].DynamicEntity.Add(player); 
        World.Inst.target = player;
        
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
                        if (Random.NextDouble() <= 0.0015)
                        {
                            SetPiece.PasteSetPiece(new Vector3Int(x, y+2, z), SetPiece.LoadSetPieceFile("house_stone"));
                        }
                        else if (Random.NextDouble() <= 0.002)
                        {
                            SetPiece.PasteSetPiece(new Vector3Int(x, y+1, z), SetPiece.LoadSetPieceFile("tree_a"));
                        }
                        else if (Random.NextDouble() <= 0.001)
                        {
                            SetPiece.PasteSetPiece(new Vector3Int(x, y+1, z), SetPiece.LoadSetPieceFile("tree_b")); 
                        } 
                    } 
                }
            }
        }
    }

    private static Chunk Generate(Vector3Int coordinates)
    {
        _chunkCoord = coordinates;
        _chunk = new Chunk();

        if (Flat)
        {
            if (coordinates.y == 0)
            {
                for (int z = 0; z < World.ChunkSize; z++)
                {
                    for (int x = 0; x < World.ChunkSize; x++)
                    {
                        _chunk[x, 0, z] = 1;
                    }
                }
            }
        }
        else
        {
            HandleBlockGeneration();
            HandleEntityGeneration(_chunk, coordinates); 
        } 
        
        return _chunk;
    }  
    
    private static void HandleBlockGeneration()
    {
        bool wall = new System.Random().NextDouble() < 0.1;
        bool[,] maze = HandleMazeAlgorithm(World.ChunkSize, World.ChunkSize);

        int centerX = World.Inst.Bounds.x / 4;
        int centerZ = World.Inst.Bounds.z / 4;
        int craterRadius = 35;  
    
        for (int y = 0; y < World.ChunkSize; y++)
        {
            for (int x = 0; x < World.ChunkSize; x++)
            {
                for (int z = 0; z < World.ChunkSize; z++)
                {
                    if (IsWithinCrater(x, y, z, centerX, centerZ, craterRadius))
                    {
                        _chunk[x, y, z] = 0;  
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
        float taperFactor = (float)(y + _chunkCoord.y) / WorldHeight;
        float taperedRadius = craterRadius * taperFactor;

        // Add noise to the crater walls
        float noiseX = (_chunkCoord.x + x) * CaveScale + _caveOffset;
        float noiseZ = (_chunkCoord.z + z) * CaveScale + _caveOffset;
        float wallNoise = Mathf.PerlinNoise(noiseX, noiseZ) * 2.0f - 1.0f; // Noise value between -1 and 1
        taperedRadius += wallNoise;

        return distanceFromCenter <= taperedRadius;
    }
    
    private static void GenerateTerrainBlocks(int x, int y, int z)
    {
        float stoneX = Mathf.Abs(_chunkCoord.x + x) * StoneScale + _stoneOffset;
        float stoneZ = Mathf.Abs(_chunkCoord.z + z) * StoneScale + _stoneOffset;
        float stoneNoiseValue = Mathf.PerlinNoise(stoneX, stoneZ);
        int stoneHeight = Mathf.FloorToInt(stoneNoiseValue * (WorldHeight / 4)) + (WorldHeight * 3 / 4);

        float dirtX = Mathf.Abs(_chunkCoord.x + x) * DirtScale + _dirtOffset;
        float dirtZ = Mathf.Abs(_chunkCoord.z + z) * DirtScale + _dirtOffset;  
        float dirtNoiseValue = Mathf.PerlinNoise(dirtX, dirtZ);
        int dirtHeight = Mathf.FloorToInt(dirtNoiseValue * (WorldHeight / 4)) + (WorldHeight * 3 / 4);

        float sandX = Mathf.Abs(_chunkCoord.x + x) * SandScale + _sandOffset;
        float sandZ = Mathf.Abs(_chunkCoord.z + z) * SandScale + _sandOffset;
        float sandNoiseValue = Mathf.PerlinNoise(sandX, sandZ);
        int sandHeight = Mathf.FloorToInt(sandNoiseValue * (WorldHeight / 4)) + (WorldHeight * 3 / 4);

        float marbleX = Mathf.Abs(_chunkCoord.x + x) * MarbleScale + _marbleOffset;
        float marbleZ = Mathf.Abs(_chunkCoord.z + z) * MarbleScale + _marbleOffset;
        float marbleNoiseValue = Mathf.PerlinNoise(marbleX, marbleZ);
        int marbleHeight = Mathf.FloorToInt(marbleNoiseValue * (WorldHeight / 4)) + (WorldHeight * 3 / 4);

        if (y + _chunkCoord.y > WallHeight + FloorHeight)
        {
            if (y + _chunkCoord.y <= marbleHeight - 50)
            {
                _chunk[x, y, z] = Block.ConvertID("marble");
            } 
            else if (y + _chunkCoord.y <= stoneHeight - 5)
            {
                _chunk[x, y, z] = Block.ConvertID("stone");
            }
            else if (y + _chunkCoord.y <= dirtHeight)
            {
                _chunk[x, y, z] = Block.ConvertID("dirt");
            }
            else if (y + _chunkCoord.y <= sandHeight)
            {
                _chunk[x, y, z] = Block.ConvertID("sand");
            }
        }
    }

    private static void GenerateCaves(int x, int y, int z)
    {
        float caveX = (_chunkCoord.x + x) * CaveScale + _caveOffset;
        float caveY = (_chunkCoord.y + y) * CaveScale + _caveOffset;
        float caveZ = (_chunkCoord.z + z) * CaveScale + _caveOffset;
        float caveNoiseValue = Mathf.PerlinNoise(caveX, caveY) * Mathf.PerlinNoise(caveY, caveZ);

        if (caveNoiseValue > 0.35f)
        {
            _chunk[x, y, z] = 0; // Empty space for caves
        }
    }
 
    private static void HandleBackroomGeneration(int x, int y, int z, bool[,] maze)
    {
        if (y + _chunkCoord.y < FloorHeight)
        {
            _chunk[x, y, z] = Block.ConvertID("backroom"); // Floor
        }
        else if (y + _chunkCoord.y == WallHeight + FloorHeight)
        {
            _chunk[x, y, z] = Block.ConvertID("backroom"); // Ceiling
        }
        else if (maze[x, z] && y + _chunkCoord.y < WallHeight + FloorHeight)
        {
            _chunk[x, y, z] = Block.ConvertID("backroom"); // Walls
        }
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
                                    else if (rng <= 0.08)
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
                        if (SpawnDynamicEntity && Random.NextDouble() <= 0.0002)
                        { 
                            if (Random.NextDouble() <= 0.5)
                            {
                                if (Random.NextDouble() <= 0.5)
                                {
                                    chunk.DynamicEntity.Add(Entity.CreateInfo("snare_flea", coordinates + new Vector3Int(x, y + 1, z)));
                                } else
                                {
                                    chunk.DynamicEntity.Add(Entity.CreateInfo("chito", coordinates + new Vector3Int(x, y + 1, z))); 
                                }
                            } else { 
                                if (Random.NextDouble() <= 0.5)
                                {
                                    chunk.DynamicEntity.Add(Entity.CreateInfo("megumin", coordinates + new Vector3Int(x, y + 1, z))); 
                                } else
                                {
                                    chunk.DynamicEntity.Add(Entity.CreateInfo("yuuri", coordinates + new Vector3Int(x, y + 1, z)));
                                }
                            } 
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
