using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using Random = UnityEngine.Random;

public class WorldGenSingleton : MonoBehaviour
{
    public static WorldGenSingleton Instance { get; private set; }
    private static System.Random _random = new System.Random();
    
    public bool SPAWN_STATIC_ENTITY; 
    public bool SPAWN_DYNAMIC_ENTITY; 
    public bool FLAT; 

    // Generate random offsets for Perlin noise
    private float _stoneOffsetX;
    private float _stoneOffsetZ;
    private float _dirtOffsetX;
    private float _dirtOffsetZ;
    private float _sandOffsetX;
    private float _sandOffsetZ;
    private float _marbleOffsetX;
    private float _marbleOffsetZ;
    private float _caveOffset;
    
    private int _chunkSize = WorldSingleton.CHUNK_SIZE;
    private int _worldHeight = ySize * WorldSingleton.CHUNK_SIZE;
    
    private float _stoneScale = 0.01f;
    private float _dirtScale = 0.005f;
    private float _sandScale = 0.02f;
    private float _marbleScale = 0.007f;
    private float _caveScale = 0.06f; // Scale for cave noise
    private int _wallHeight = 5;
    private int _floorHeight = 2;

    private ChunkData _chunkData;
    private Vector3Int _chunkCoord;
    
    private Vector3Int _positionA;
    private Vector3Int _positionB;
    private ChunkData _setPiece;
    
    private void Awake() {
        Instance = this;
        _stoneOffsetX = Random.Range(0f, 1000f);
        _stoneOffsetZ = Random.Range(0f, 1000f);
        _dirtOffsetX = Random.Range(0f, 1000f);
        _dirtOffsetZ = Random.Range(0f, 1000f);
        _sandOffsetX = Random.Range(0f, 1000f);
        _sandOffsetZ = Random.Range(0f, 1000f);
        _marbleOffsetX = Random.Range(0f, 1000f);
        _marbleOffsetZ = Random.Range(0f, 1000f);
        _caveOffset = Random.Range(0f, 1000f); 
    }




    public static int xSize = 5;
    public static int ySize = 2;
    public static int zSize = 5;


    //! debug tools
    public void GenerateRandomMapSave()
    { 
        WorldSingleton.World = new WorldData(xSize, ySize, zSize);
        List<Vector3Int> coordinatesList = new List<Vector3Int>();

        for (int x = 0; x < xSize * WorldSingleton.CHUNK_SIZE; x += WorldSingleton.CHUNK_SIZE)
        {
            for (int y = 0; y < ySize * WorldSingleton.CHUNK_SIZE; y += WorldSingleton.CHUNK_SIZE)
            {
                for (int z = 0; z < zSize * WorldSingleton.CHUNK_SIZE; z += WorldSingleton.CHUNK_SIZE)
                {
                    coordinatesList.Add(new Vector3Int(x, y, z));
                }
            }
        } 
        
        foreach (var coordinates in coordinatesList)
        {
            WorldSingleton.World[coordinates.x, coordinates.y, coordinates.z] = GenerateTestChunk(coordinates); 
        }
        
        
        ChunkData house = LoadSetPieceFile("house_stone");
        ChunkData tree = LoadSetPieceFile("tree_a");

        int chunkSize = WorldSingleton.CHUNK_SIZE;
        for (int x = 0; x < WorldSingleton.World.Bounds.x; x++)
        {
            for (int y = 0; y < WorldSingleton.World.Bounds.y - 1; y++)
            {
                for (int z = 0; z < WorldSingleton.World.Bounds.z; z++)
                { 
                    Vector3Int worldPos = new Vector3Int(x, y, z);
                    Vector3Int chunkPos = WorldSingleton.Instance.GetRelativePosition(worldPos);
                    int localChunkX = worldPos.x % chunkSize;
                    int localChunkY = worldPos.y % chunkSize;
                    int localChunkZ = worldPos.z % chunkSize;

                    if (WorldSingleton.World[chunkPos.x, chunkPos.y, chunkPos.z]
                            .Map[localChunkX, localChunkY, localChunkZ] == BlockSingleton.ConvertID("dirt") &&
                        WorldSingleton.World[chunkPos.x, chunkPos.y, chunkPos.z]
                            .Map[localChunkX, localChunkY, localChunkZ] == BlockSingleton.ConvertID("dirt"))
                    {

                        if (_random.NextDouble() <= 0.0002)
                        {
                            _setPiece = house;
                            pasteSetPiece(new Vector3Int(x, y, z));
                        }
                        else if (_random.NextDouble() <= 0.0004)
                        {
                            _setPiece = tree;
                            pasteSetPiece(new Vector3Int(x, y, z));
                        }
                    } 
                }
            }
        }
    }  

    public ChunkData GenerateTestChunk(Vector3Int coordinates)
    {
        _chunkCoord = coordinates;
        _chunkData = new ChunkData();

        if (FLAT)
        {
            if (coordinates.y == 0)
            {
                for (int z = 0; z < WorldSingleton.CHUNK_SIZE; z++)
                {
                    for (int x = 0; x < WorldSingleton.CHUNK_SIZE; x++)
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
    
    private void HandleBlockGeneration()
    {
        bool wall = new System.Random().NextDouble() < 0.1;
        bool[,] maze = HandleMazeAlgorithm(_chunkSize, _chunkSize);

        // Calculate the center of the map
        int centerX = WorldSingleton.World.Bounds.x / 2;
        int centerZ = WorldSingleton.World.Bounds.z / 2;
        int craterRadius = WorldSingleton.World.Bounds.z / 3; // Adjust the radius as needed
    
        for (int y = 0; y < _chunkSize; y++)
        {
            for (int x = 0; x < _chunkSize; x++)
            {
                for (int z = 0; z < _chunkSize; z++)
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
    private bool IsWithinCrater(int x, int y, int z, int centerX, int centerZ, int craterRadius)
    {
        int distanceX = Mathf.Abs(centerX - (_chunkCoord.x + x));
        int distanceZ = Mathf.Abs(centerZ - (_chunkCoord.z + z));
        float distanceFromCenter = Mathf.Sqrt(distanceX * distanceX + distanceZ * distanceZ);

        // Calculate the taper factor so that it decreases with depth
        float taperFactor = (float)(y + _chunkCoord.y) / _worldHeight;
        float taperedRadius = craterRadius * taperFactor;

        // Add noise to the crater walls
        float noiseX = (_chunkCoord.x + x) * _caveScale + _caveOffset;
        float noiseZ = (_chunkCoord.z + z) * _caveScale + _caveOffset;
        float wallNoise = Mathf.PerlinNoise(noiseX, noiseZ) * 2.0f - 1.0f; // Noise value between -1 and 1
        taperedRadius += wallNoise;

        return distanceFromCenter <= taperedRadius;
    }
    
    private void GenerateTerrainBlocks(int x, int y, int z)
    {
        float stoneX = Mathf.Abs(_chunkCoord.x + x) * _stoneScale + _stoneOffsetX;
        float stoneZ = Mathf.Abs(_chunkCoord.z + z) * _stoneScale + _stoneOffsetZ;
        float stoneNoiseValue = Mathf.PerlinNoise(stoneX, stoneZ);
        int stoneHeight = Mathf.FloorToInt(stoneNoiseValue * (_worldHeight / 4)) + (_worldHeight * 3 / 4);

        float dirtX = Mathf.Abs(_chunkCoord.x + x) * _dirtScale + _dirtOffsetX;
        float dirtZ = Mathf.Abs(_chunkCoord.z + z) * _dirtScale + _dirtOffsetZ;  
        float dirtNoiseValue = Mathf.PerlinNoise(dirtX, dirtZ);
        int dirtHeight = Mathf.FloorToInt(dirtNoiseValue * (_worldHeight / 4)) + (_worldHeight * 3 / 4);

        float sandX = Mathf.Abs(_chunkCoord.x + x) * _sandScale + _sandOffsetX;
        float sandZ = Mathf.Abs(_chunkCoord.z + z) * _sandScale + _sandOffsetZ;
        float sandNoiseValue = Mathf.PerlinNoise(sandX, sandZ);
        int sandHeight = Mathf.FloorToInt(sandNoiseValue * (_worldHeight / 4)) + (_worldHeight * 3 / 4);

        float marbleX = Mathf.Abs(_chunkCoord.x + x) * _marbleScale + _marbleOffsetX;
        float marbleZ = Mathf.Abs(_chunkCoord.z + z) * _marbleScale + _marbleOffsetZ;
        float marbleNoiseValue = Mathf.PerlinNoise(marbleX, marbleZ);
        int marbleHeight = Mathf.FloorToInt(marbleNoiseValue * (_worldHeight / 4)) + (_worldHeight * 3 / 4);

        if (y + _chunkCoord.y > _wallHeight + _floorHeight)
        {
            if (y + _chunkCoord.y <= marbleHeight - 50)
            {
                _chunkData.Map[x, y, z] = BlockSingleton.ConvertID("marble");
            } 
            else if (y + _chunkCoord.y <= stoneHeight - 5)
            {
                _chunkData.Map[x, y, z] = BlockSingleton.ConvertID("stone");
            }
            else if (y + _chunkCoord.y <= dirtHeight)
            {
                _chunkData.Map[x, y, z] = BlockSingleton.ConvertID("dirt");
            }
            else if (y + _chunkCoord.y <= sandHeight)
            {
                _chunkData.Map[x, y, z] = BlockSingleton.ConvertID("sand");
            }
        }
    }

    private void GenerateCaves(int x, int y, int z)
    {
        float caveX = (_chunkCoord.x + x) * _caveScale + _caveOffset;
        float caveY = (_chunkCoord.y + y) * _caveScale + _caveOffset;
        float caveZ = (_chunkCoord.z + z) * _caveScale + _caveOffset;
        float caveNoiseValue = Mathf.PerlinNoise(caveX, caveY) * Mathf.PerlinNoise(caveY, caveZ);

        if (caveNoiseValue > 0.35f)
        {
            _chunkData.Map[x, y, z] = 0; // Empty space for caves
        }
    }
 
    private void HandleBackroomGeneration(int x, int y, int z, bool[,] maze)
    {
        if (y + _chunkCoord.y < _floorHeight)
        {
            _chunkData.Map[x, y, z] = BlockSingleton.ConvertID("backroom"); // Floor
        }
        else if (y + _chunkCoord.y == _wallHeight + _floorHeight)
        {
            _chunkData.Map[x, y, z] = BlockSingleton.ConvertID("backroom"); // Ceiling
        }
        else if (maze[x, z] && y + _chunkCoord.y < _wallHeight + _floorHeight)
        {
            _chunkData.Map[x, y, z] = BlockSingleton.ConvertID("backroom"); // Walls
        }
    }
 
    private void HandleEntityGeneration(ChunkData chunkData)
    { 
        EntityData entityData;
        SerializableVector3 entityPosition;
        
        for (int x = 0; x < _chunkSize; x++)
        {
            for (int y = 0; y < _chunkSize; y++)
            {
                for (int z = 0; z <  _chunkSize; z++)
                {
                    if (
                        y + 1 < _chunkSize &&
                        chunkData.Map[x, y, z] != 0 && 
                        chunkData.Map[x, y + 1, z] == 0) 
                    {
                        if (SPAWN_STATIC_ENTITY)
                        {
                            if (chunkData.Map[x, y, z] == BlockSingleton.ConvertID("dirt"))
                            {
                                {
                                    double rng = _random.NextDouble();
                                    if (rng <= 0.03)
                                    {
                                        entityPosition = new SerializableVector3(x + 0.5f, y + 1, z + 0.5f);
                                        entityData = new EntityData("tree", entityPosition,
                                            new SerializableVector3Int(1, 3, 1));
                                        chunkData.StaticEntity.Add(entityData);
                                    }
                                    else if (rng <= 0.03)
                                    {
                                        entityPosition = new SerializableVector3(x + 0.5f, y + 1, z + 0.5f);
                                        entityData = new EntityData("bush1", entityPosition,
                                            new SerializableVector3Int(0, 0, 0));
                                        chunkData.StaticEntity.Add(entityData);
                                    }
                                    else if (rng <= 0.2)
                                    {
                                        entityPosition = new SerializableVector3(x + (float)_random.NextDouble() / 1.5f,
                                            y + 1, z + (float)_random.NextDouble() / 1.5f);
                                        entityData = new EntityData("grass", entityPosition);
                                        chunkData.StaticEntity.Add(entityData);
                                    }
                                }
                            }
                            else
                            {
                                if (_random.NextDouble() <= 0.003)
                                {

                                    entityPosition = new SerializableVector3(x + 0.5f, y + 1, z + 0.5f);
                                    entityData = new EntityData("stage_hand", entityPosition,
                                        new SerializableVector3Int(1, 1, 1));
                                    chunkData.StaticEntity.Add(entityData);
                                }
                                else if (_random.NextDouble() <= 0.03)
                                {

                                    entityPosition = new SerializableVector3(x + 0.5f, y + 1, z + 0.5f);
                                    entityData = new EntityData("slab", entityPosition,
                                        new SerializableVector3Int(0, 0, 0));
                                    chunkData.StaticEntity.Add(entityData);
                                }
                            }
 
                        }
                        if (SPAWN_DYNAMIC_ENTITY && _random.NextDouble() <= 0.003)
                        {
                            entityPosition = new SerializableVector3(x + 0.5f, y + 1, z +0.5f);
                            if (_random.NextDouble() <= 0.5)
                            {
                                if (_random.NextDouble() <= 0.5)
                                {
                                    entityData = new EntityData("snare_flea", entityPosition, type: EntityType.Rigid);
                                } else
                                {
                                    entityData = new EntityData("chito", entityPosition, type: EntityType.Rigid);
                                }
                            } else { 
                                if (_random.NextDouble() <= 0.5)
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
    
    private bool[,] HandleMazeAlgorithm(int width, int height)
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
 
    

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            InventorySingleton.AddItem("marble", 100);
            InventorySingleton.AddItem("backroom", 100);
        }
        if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            Lib.Log("anchor A set");
            _positionA = Vector3Int.FloorToInt(Game.Player.transform.position);
        }

        if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            Lib.Log("anchor B set");
            _positionB = Vector3Int.FloorToInt(Game.Player.transform.position);
        } 
    
        if (Input.GetKeyDown(KeyCode.Equals))
        {
            Lib.Log("exported to file");
            copySetPiece();
            SaveSetPieceFile(_setPiece, "tree_a");
        }
        if (Input.GetKeyDown(KeyCode.Minus))
        {
            Lib.Log("imported to world"); 
            pasteSetPiece(Vector3Int.FloorToInt(Game.Player.transform.position));
        }
    }

    void SaveSetPieceFile(ChunkData data, string fileName)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Path.Combine(Application.dataPath, "Resources/set", fileName + ".dat");
        using (FileStream stream = new FileStream(path, FileMode.Create))
        {
            formatter.Serialize(stream, data);
        }
    }

    ChunkData LoadSetPieceFile(string fileName)
    {
        string path = Path.Combine(Application.dataPath, "Resources/set", fileName + ".dat");
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream stream = new FileStream(path, FileMode.Open))
            {
                return (ChunkData)formatter.Deserialize(stream);
            }
        }
        return null;
    }
    
    private void copySetPiece()
    {
        int minX = Mathf.Min(_positionA.x, _positionB.x);
        int minY = Mathf.Min(_positionA.y, _positionB.y);
        int minZ = Mathf.Min(_positionA.z, _positionB.z);
        int maxX = Mathf.Max(_positionA.x, _positionB.x);
        int maxY = Mathf.Max(_positionA.y, _positionB.y);
        int maxZ = Mathf.Max(_positionA.z, _positionB.z);
        
        _setPiece = new ChunkData(Mathf.Max(maxX,maxY,maxZ));
        Vector3Int min = new Vector3Int(minX, minY, minZ);
        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                for (int z = minZ; z <= maxZ; z++)
                {
                    Vector3Int worldPos = new Vector3Int(x, y, z);
                    Vector3Int localPos = worldPos - min;

                    Vector3Int chunkPos = WorldSingleton.Instance.GetRelativePosition(worldPos);
                    int localChunkX = worldPos.x % WorldSingleton.CHUNK_SIZE;
                    int localChunkY = worldPos.y % WorldSingleton.CHUNK_SIZE;
                    int localChunkZ = worldPos.z % WorldSingleton.CHUNK_SIZE;
                
                    _setPiece.Map[localPos.x, localPos.y, localPos.z] = WorldSingleton.World[chunkPos.x, chunkPos.y, chunkPos.z].Map[localChunkX, localChunkY, localChunkZ];
                }
            }
        }
    }
    
    private void pasteSetPiece(Vector3Int position)
    {
        int chunkSize = WorldSingleton.CHUNK_SIZE;
        Vector3Int min = position;

        for (int x = 0; x < _setPiece.Map.GetLength(0); x++)
        {
            for (int y = 0; y < _setPiece.Map.GetLength(1); y++)
            {
                for (int z = 0; z < _setPiece.Map.GetLength(2); z++)
                {
                    int blockID = _setPiece.Map[x, y, z];
                    if (blockID != 0)
                    {
                        Vector3Int worldPos = new Vector3Int(min.x + x, min.y + y, min.z + z);
                        Vector3Int chunkPos = WorldSingleton.Instance.GetRelativePosition(worldPos);
                        int localChunkX = worldPos.x % chunkSize;
                        int localChunkY = worldPos.y % chunkSize;
                        int localChunkZ = worldPos.z % chunkSize;

                        if (WorldSingleton.Instance.IsInWorldBounds(worldPos))
                            WorldSingleton.World[chunkPos.x, chunkPos.y, chunkPos.z].Map[localChunkX, localChunkY, localChunkZ] = blockID;
                    }
                }
            }
        }
    }
}
