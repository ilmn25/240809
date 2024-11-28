using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenStatic : MonoBehaviour
{
    public static WorldGenStatic Instance { get; private set; }  
    
    private int _chunkSize;
    public bool SPAWN_ENTITY; 

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
        _chunkSize = WorldStatic.CHUNKSIZE;
        ChunkData chunkData = new ChunkData();
        HandleBlockGeneration();
        if (SPAWN_ENTITY) HandleEntityGeneration();
        return chunkData; 

        void HandleBlockGeneration()
        {
            float terrainScale = 0.07f; // Adjust the scale to change the frequency of the terrain noise
            float caveScale = 0.15f; // Adjust the scale to change the frequency of the cave noise
            float caveThreshold = 0.15f; // Adjust the threshold to change the density of the caves
            float marbleScale = 0.02f; // Adjust the scale to change the frequency of the marble noise
            int wallHeight = 4; // Height of the maze walls
            int floorHeight = 1; // Height of the floor
            bool wall = new System.Random().NextDouble() < 0.1;
 

            // Updated maze generation logic
            bool[,] maze = HandleMazeAlgorithm(_chunkSize, _chunkSize);

            for (int x = 0; x < _chunkSize; x++)
            {
                for (int z = 0; z < _chunkSize; z++)
                {
                    // Normalize coordinates to ensure they are always positive
                    float worldX = Mathf.Abs(coordinates.x + x + 100) * terrainScale + terrainOffsetX;
                    float worldZ = Mathf.Abs(coordinates.z + z + 100) * terrainScale + terrainOffsetZ;

                    // Generate Perlin noise value for terrain
                    float perlinValue = Mathf.PerlinNoise(worldX, worldZ);
                    // Scale the Perlin noise value to the range [0, _chunkSize]
                    int terrainHeight = Mathf.FloorToInt(perlinValue * _chunkSize);

                    // Generate Perlin noise value for marble layer height
                    float marbleNoiseValue = Mathf.PerlinNoise(Mathf.Abs(coordinates.x + x) * marbleScale + marbleOffsetX, Mathf.Abs(coordinates.z + z) * marbleScale + marbleOffsetZ);
                    int marbleLayerHeight = Mathf.FloorToInt(marbleNoiseValue * _chunkSize);

                    for (int y = 0; y < _chunkSize; y++)
                    {
                        // Normalize coordinates for caves
                        float caveX = Mathf.Abs(coordinates.x + x) * caveScale + caveOffsetX;
                        float caveY = Mathf.Abs(coordinates.y + y) * caveScale;
                        float caveZ = Mathf.Abs(coordinates.z + z) * caveScale + caveOffsetZ;

                        // Generate Perlin noise value for caves
                        float caveValue = Mathf.PerlinNoise(caveX, caveY) * Mathf.PerlinNoise(caveY, caveZ);

                        if (y < floorHeight)
                        {
                            chunkData.Map[x, y, z] = BlockStatic.ConvertID("backroom"); // Floor
                        }
                        else if (y == wallHeight + floorHeight)
                        {
                            chunkData.Map[x, y, z] = BlockStatic.ConvertID("backroom"); // Ceiling
                        }
                        else if (maze[x, z] && y < wallHeight + floorHeight)
                        {
                            chunkData.Map[x, y, z] = BlockStatic.ConvertID("backroom"); // Walls
                        }
                        if (y > wallHeight + floorHeight) {
                            if (wall & (z == 1 || z == 2) && y < 17)
                            {
                                chunkData.Map[x, y, z] = BlockStatic.ConvertID("brick");
                            } 
                            else if (y <= terrainHeight && caveValue > caveThreshold)
                            {
                                chunkData.Map[x, y, z] = BlockStatic.ConvertID("stone");
                            }
                            else if (y < marbleLayerHeight - 4)
                            {
                                chunkData.Map[x, y, z] = BlockStatic.ConvertID("dirt");
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
            for (int x = 0; x < WorldStatic.CHUNKSIZE; x++)
            {
                for (int y = 0; y < _chunkSize; y++)
                {
                    for (int z = 0; z <  WorldStatic.CHUNKSIZE; z++)
                    {
                        if (chunkData.Map[x, y, z] == BlockStatic.ConvertID("dirt"))
                        {
                            if (y + 1 < _chunkSize && chunkData.Map[x, y + 1, z] == 0) // 5% chance
                            {
                                double rng = random.NextDouble();
                                if (rng <= 0.03)
                                {
                                    entityPosition = new SerializableVector3(x + 0.5f, y + 1, z +0.5f);
                                    entityData = new EntityData("tree", entityPosition, new SerializableVector3Int(1, 3, 1));
                                    chunkData.StaticEntity.Add(entityData);
                                } else if (rng <= 0.07)
                                {
                                    entityPosition = new SerializableVector3(x + 0.5f, y + 1, z +0.5f);
                                    entityData = new EntityData("bush1", entityPosition, new SerializableVector3Int(0, 0, 0));
                                    chunkData.StaticEntity.Add(entityData);
                                } else if (rng <= 0.35)
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
                                if (random.NextDouble() <= 0.004)
                                { 

                                    entityPosition = new SerializableVector3(x + 0.5f, y + 1, z +0.5f);
                                    entityData = new EntityData("stage_hand", entityPosition, new SerializableVector3Int(1, 1, 1));
                                    chunkData.StaticEntity.Add(entityData);
                                }
                                else if (random.NextDouble() <= 0.05)
                                { 

                                    entityPosition = new SerializableVector3(x + 0.5f, y + 1, z +0.5f);
                                    entityData = new EntityData("slab", entityPosition, new SerializableVector3Int(0, 0, 0));
                                    chunkData.StaticEntity.Add(entityData);
                                }
                                else if (random.NextDouble() <= 0.003)
                                {
                                    entityPosition = new SerializableVector3(x + 0.5f, y + 2f, z +0.5f);
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
