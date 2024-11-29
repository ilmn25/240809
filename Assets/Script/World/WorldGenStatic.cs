using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenStatic : MonoBehaviour
{
    public static WorldGenStatic Instance { get; private set; }  
    
    private int _chunkSize;
    public bool SPAWN_STATIC_ENTITY; 
    public bool SPAWN_DYNAMIC_ENTITY; 

    // Generate random offsets for Perlin noise
    float terrainOffsetX;
    float terrainOffsetZ;
    float marbleOffsetX;
    float marbleOffsetZ;
    float caveOffsetX; 
    float caveOffsetZ;

    private void Awake() {
        Instance = this;
        terrainOffsetX = Random.Range(0f, 1000f);
        terrainOffsetZ = Random.Range(0f, 1000f);
        marbleOffsetX = Random.Range(0f, 1000f);
        marbleOffsetZ = Random.Range(0f, 1000f);
        caveOffsetX = Random.Range(0f, 1000f); 
        caveOffsetZ = Random.Range(0f, 1000f);
    }

    public ChunkData GenerateTestChunk(Vector3Int coordinates)
    {
        _chunkSize = WorldStatic.CHUNK_SIZE;
        ChunkData chunkData = new ChunkData();
        HandleBlockGeneration();
        HandleEntityGeneration();
        return chunkData;

        void HandleBlockGeneration()
        {
            int worldHeight = WorldStatic.ySize * WorldStatic.CHUNK_SIZE;

            float stoneScale = 0.03f;
            float dirtScale = 0.02f;
            float caveScale = 0.05f; // Scale for cave noise

            int wallHeight = 5;
            int floorHeight = 2;
            bool wall = new System.Random().NextDouble() < 0.1;
            bool[,] maze = HandleMazeAlgorithm(_chunkSize, _chunkSize);

            for (int y = 0; y < _chunkSize; y++)
            {
                for (int x = 0; x < _chunkSize; x++)
                {
                    for (int z = 0; z < _chunkSize; z++)
                    {
                        float stoneX = Mathf.Abs(coordinates.x + x) * stoneScale + terrainOffsetX;
                        float stoneZ = Mathf.Abs(coordinates.z + z) * stoneScale + terrainOffsetZ;
                        float stoneNoiseValue = Mathf.PerlinNoise(stoneX, stoneZ);
                        int stoneHeight = Mathf.FloorToInt(stoneNoiseValue * worldHeight);

                        float dirtX = Mathf.Abs(coordinates.x + x) * dirtScale + terrainOffsetZ;
                        float dirtz = Mathf.Abs(coordinates.z + z) * dirtScale + terrainOffsetX; // swapped
                        float dirtNoiseValue = Mathf.PerlinNoise(dirtX, dirtz);
                        int dirtHeight = Mathf.FloorToInt(dirtNoiseValue * worldHeight);

                        float caveX = (coordinates.x + x) * caveScale;
                        float caveY = (coordinates.y + y) * caveScale;
                        float caveZ = (coordinates.z + z) * caveScale;
                        float caveNoiseValue = Mathf.PerlinNoise(caveX, caveY) * Mathf.PerlinNoise(caveY, caveZ);

                        if (caveNoiseValue > 0.5f)
                        {
                            chunkData.Map[x, y, z] = 0; // Empty space for caves
                        }
                        else
                        {
                            if (y + coordinates.y < floorHeight)
                            {
                                chunkData.Map[x, y, z] = BlockStatic.ConvertID("backroom"); // Floor
                            }
                            else if (y + coordinates.y == wallHeight + floorHeight)
                            {
                                chunkData.Map[x, y, z] = BlockStatic.ConvertID("backroom"); // Ceiling
                            }
                            else if (maze[x, z] && y + coordinates.y < wallHeight + floorHeight)
                            {
                                chunkData.Map[x, y, z] = BlockStatic.ConvertID("backroom"); // Walls
                            }

                            if (y + coordinates.y > wallHeight + floorHeight)
                            {
                                if (wall & (z > 1 && z < 8) && y + coordinates.y < 100)
                                {
                                    chunkData.Map[x, y, z] = BlockStatic.ConvertID("brick");
                                }
                                if (y + coordinates.y <= stoneHeight - 15)
                                {
                                    chunkData.Map[x, y, z] = BlockStatic.ConvertID("stone");
                                }
                                else if (y + coordinates.y <= dirtHeight)
                                {
                                    chunkData.Map[x, y, z] = BlockStatic.ConvertID("dirt");
                                }
                            }
                        }
                    }
                }
            }
        }


        void HandleEntityGeneration()
        {
            System.Random random = new System.Random();
            EntityData entityData;
            SerializableVector3 entityPosition;
            for (int x = 0; x < WorldStatic.CHUNK_SIZE; x++)
            {
                for (int y = 0; y < _chunkSize; y++)
                {
                    for (int z = 0; z <  WorldStatic.CHUNK_SIZE; z++)
                    {
                        if (SPAWN_STATIC_ENTITY && chunkData.Map[x, y, z] == BlockStatic.ConvertID("dirt"))
                        {
                            if (y + 1 < _chunkSize && chunkData.Map[x, y + 1, z] == 0) // 5% chance
                            {
                                double rng = random.NextDouble();
                                if (rng <= 0.03)
                                {
                                    entityPosition = new SerializableVector3(x + 0.5f, y + 1, z +0.5f);
                                    entityData = new EntityData("tree", entityPosition, new SerializableVector3Int(1, 3, 1));
                                    chunkData.StaticEntity.Add(entityData);
                                } else if (rng <= 0.03)
                                {
                                    entityPosition = new SerializableVector3(x + 0.5f, y + 1, z +0.5f);
                                    entityData = new EntityData("bush1", entityPosition, new SerializableVector3Int(0, 0, 0));
                                    chunkData.StaticEntity.Add(entityData);
                                } else if (rng <= 0.2)
                                {
                                    entityPosition = new SerializableVector3(x + (float)random.NextDouble()/1.5f, y+1, z + (float)random.NextDouble()/1.5f);
                                    entityData = new EntityData("grass", entityPosition);
                                    chunkData.StaticEntity.Add(entityData);
                                }
                            }
                        } 
                        else if (chunkData.Map[x, y, z] != 0)
                        {
                            if (y + 1 < _chunkSize && chunkData.Map[x, y + 1, z] == 0) // 5% chance
                            {
                                if (SPAWN_STATIC_ENTITY && random.NextDouble() <= 0.003)
                                { 

                                    entityPosition = new SerializableVector3(x + 0.5f, y + 1, z +0.5f);
                                    entityData = new EntityData("stage_hand", entityPosition, new SerializableVector3Int(1, 1, 1));
                                    chunkData.StaticEntity.Add(entityData);
                                }
                                else if (SPAWN_STATIC_ENTITY && random.NextDouble() <= 0.03)
                                { 

                                    entityPosition = new SerializableVector3(x + 0.5f, y + 1, z +0.5f);
                                    entityData = new EntityData("slab", entityPosition, new SerializableVector3Int(0, 0, 0));
                                    chunkData.StaticEntity.Add(entityData);
                                }
                                else if (SPAWN_DYNAMIC_ENTITY && random.NextDouble() <= 0.003)
                                {
                                    entityPosition = new SerializableVector3(x + 0.5f, y + 1, z +0.5f);
                                    if (random.NextDouble() <= 0.5)
                                    {
                                        if (random.NextDouble() <= 0.5)
                                        {
                                            entityData = new EntityData("snare_flea", entityPosition, type: EntityType.Rigid);
                                        } else
                                        {
                                            entityData = new EntityData("chito", entityPosition, type: EntityType.Rigid);
                                        }
                                    } else { 
                                        if (random.NextDouble() <= 0.5)
                                        {
                                            entityData = new EntityData("megumin", entityPosition, type: EntityType.Rigid);
                                        } else
                                        {
                                            entityData = new EntityData("yuuri", entityPosition, type: EntityType.Rigid);
                                        }
                                    }
                                    chunkData.DynamicEntity.Add(entityData); 
                                }
                            } 
                        }
                    }
                }
            }
        }
        
        bool[,] HandleMazeAlgorithm(int width, int height)
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

}
