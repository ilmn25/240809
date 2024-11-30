using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class WorldStatic : MonoBehaviour
{ 
    public static WorldStatic Instance { get; private set; }  
    
    public static WorldData World;

    private static BinaryFormatter _bf = new BinaryFormatter(); 
    public static event Action PlayerChunkTraverse;
    public delegate void Vector3IntEvent(Vector3Int position); 
    public static event Vector3IntEvent MapUpdated;

    [HideInInspector] 
    public static Vector3Int _playerChunkPos;
    [HideInInspector] 
    public static Vector3Int _chunkPositionPrevious = Vector3Int.one;
    [HideInInspector] 
    public static Vector3Int _boolMapOrigin = Vector3Int.zero;
    [HideInInspector] 
    public static bool[,,] _boolMap;
    [HideInInspector] 
    public static int CHUNK_SIZE = 20; 
    [HideInInspector] 
    public static int RENDER_DISTANCE = 2; 
    public static bool ALWAYS_REGENERATE = false;
 
    private void Awake()    
    {
        Instance = this;
    }

    void Start()
    { 
        Instance = this;
 
        if (!File.Exists(getFilePath(0)) || ALWAYS_REGENERATE) WorldGenStatic.Instance.GenerateRandomMapSave();
        _chunkPositionPrevious = GetChunkCoordinate(Game.Player.transform.position); 
    }
          
    void FixedUpdate()
    {
        _playerChunkPos = GetChunkCoordinate(Game.Player.transform.position);
        if (_playerChunkPos != _chunkPositionPrevious)
        {  
            PlayerChunkTraverse?.Invoke();
            _chunkPositionPrevious = _playerChunkPos; 
        }
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleSaveWorldFile(0);
            Application.Quit();
        }
    }

    private void OnApplicationQuit()
    {
        HandleSaveWorldFile(0);
    }

    private string getFilePath(int yLevel)
    {
        return $"{Game.DOWNLOAD_PATH}\\World{yLevel}.dat";
    }
    
    public async void HandleSaveWorldFile(int yLevel)
    {
        EntityLoadStatic.Instance.SaveAll();
        EntityLoadDynamic.Instance.SaveAll();
        await Task.Delay(10);
        
        using (FileStream file = File.Create(getFilePath(yLevel)))
        {
            _bf.Serialize(file, World);
        } 
    }
 
    public void HandleLoadWorldFile(int yLevel)
    { 
        if (World == null)
        {
            if (File.Exists(getFilePath(yLevel)))
            { 
                FileStream file = File.Open(getFilePath(yLevel), FileMode.Open);

                World = (WorldData)_bf.Deserialize(file);
                file.Close();
            } 
            else
            {
                Lib.Log("doesn't exist, load region fail" + yLevel); 
                World = new WorldData(1,1,1); // if doesn't exist
            }
        }
    }






 
    public void GenerateBoolMap()
    {
        _boolMap = new bool[World.Bounds.x, World.Bounds.y, World.Bounds.z];
        ChunkData chunkData;
        
        for (int wx = 0; wx < World.Length.x; wx++)
        { 
            for (int wy = 0; wy < World.Length.y; wy++)
            {
                for (int wz = 0; wz < World.Length.z; wz++)
                {
                    int chunkX = wx * CHUNK_SIZE;
                    int chunkY = wy * CHUNK_SIZE;
                    int chunkZ = wz * CHUNK_SIZE;
                    chunkData = World[chunkX, chunkY, chunkZ]; 
                    if (chunkData != null)
                    { 
                        for (int i = 0; i < CHUNK_SIZE; i++)
                        {
                            for (int j = 0; j < CHUNK_SIZE; j++)
                            {
                                for (int k = 0; k < CHUNK_SIZE; k++)
                                {
                                        _boolMap[chunkX + i, chunkY + j, chunkZ + k] = chunkData.Map[i, j, k] == 0;

                                }
                            }
                        }
                        foreach (var entity in chunkData.StaticEntity)
                        {
                            int entityX = chunkX + Mathf.FloorToInt(entity.Position.x);
                            int entityY = chunkY + Mathf.FloorToInt(entity.Position.y);
                            int entityZ = chunkZ + Mathf.FloorToInt(entity.Position.z);

                            int entityEndX = entityX + entity.Bounds.x;
                            int entityEndY = entityY + entity.Bounds.y;
                            int entityEndZ = entityZ + entity.Bounds.z;

                            for (int x = entityX; x < entityEndX; x++)
                            {
                                for (int y = entityY; y < entityEndY; y++)
                                {
                                    for (int z = entityZ; z < entityEndZ; z++)
                                    {
                                        if (IsInWorldBounds(x, y, z))
                                        {
                                            _boolMap[x, y, z] = false;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        } 
    }


    public Vector3Int GetRelativePosition(Vector3Int coordinate)
    { 
        return new Vector3Int(
            coordinate.x - _boolMapOrigin.x,
            coordinate.y - _boolMapOrigin.y,
            coordinate.z - _boolMapOrigin.z
        );
    }
    
    public bool GetBoolInMap(Vector3Int worldPosition)
    {
        if (!IsInWorldBounds(worldPosition)) return true;
        
        return _boolMap[worldPosition.x - _boolMapOrigin.x,
            worldPosition.y - _boolMapOrigin.y,
            worldPosition.z - _boolMapOrigin.z];
    }
    
    public void SetBoolInMap(Vector3Int worldPosition, bool value)
    {
        if (!IsInWorldBounds(worldPosition))
        {
            return;
        }
        
        _boolMap[worldPosition.x - _boolMapOrigin.x,
            worldPosition.y - _boolMapOrigin.y,
            worldPosition.z - _boolMapOrigin.z] = value;
        
        if (MapUpdated != null)
            MapUpdated(worldPosition);
    }
    
    public Boolean IsInWorldBounds(Vector3Int worldPosition)
    {
        if (worldPosition.x < World.Bounds.x && worldPosition.x >= 0 &&
            worldPosition.y < World.Bounds.y && worldPosition.y >= 0 &&
            worldPosition.z < World.Bounds.z && worldPosition.z >= 0)
            return true;
        return false;
    }
    
    public Boolean IsInWorldBounds(int x, int y, int z)
    {
        if (x < World.Bounds.x && x >= 0 &&
            y < World.Bounds.y && y >= 0 &&
            z < World.Bounds.z && z >= 0)
            return true;
        return false;
    }
    
    public void UpdateMap(Vector3Int worldCoordinate, Vector3Int chunkCoordinate, Vector3Int blockCoordinate, int blockID = 0)
    {
        AudioSingleton.PlaySFX(Game.DigSound);
        GetChunk(chunkCoordinate).Map[blockCoordinate.x, blockCoordinate.y, blockCoordinate.z] = blockID;
        SetBoolInMap(worldCoordinate, blockID == 0);
        
        MapLoadStatic.Instance.RefreshExistingChunk(chunkCoordinate); // Refresh on screen
        if (blockCoordinate.x != 0 && blockCoordinate.x != CHUNK_SIZE - 1 &&
            blockCoordinate.y != 0 && blockCoordinate.y != CHUNK_SIZE - 1 &&
            blockCoordinate.z != 0 && blockCoordinate.z != CHUNK_SIZE - 1)
            return;
        
        // X-axis checks
        if (blockCoordinate.x == 0)
            MapLoadStatic.Instance.RefreshExistingChunk(Lib.AddToVector(chunkCoordinate, -CHUNK_SIZE, 0, 0));
        else if (blockCoordinate.x == CHUNK_SIZE - 1)
            MapLoadStatic.Instance.RefreshExistingChunk(Lib.AddToVector(chunkCoordinate, CHUNK_SIZE, 0, 0));

        // Y-axis checks
        if (blockCoordinate.y == 0)
            MapLoadStatic.Instance.RefreshExistingChunk(Lib.AddToVector(chunkCoordinate, 0, -CHUNK_SIZE, 0));
        else if (blockCoordinate.y == CHUNK_SIZE - 1)
            MapLoadStatic.Instance.RefreshExistingChunk(Lib.AddToVector(chunkCoordinate, 0, CHUNK_SIZE, 0));

        // Z-axis checks
        if (blockCoordinate.z == 0)
            MapLoadStatic.Instance.RefreshExistingChunk(Lib.AddToVector(chunkCoordinate, 0, 0, -CHUNK_SIZE));
        else if (blockCoordinate.z == CHUNK_SIZE - 1)
            MapLoadStatic.Instance.RefreshExistingChunk(Lib.AddToVector(chunkCoordinate, 0, 0, CHUNK_SIZE));

        // Edge and Corner checks on X, Y, and Z axes
        if (blockCoordinate.x == 0 && blockCoordinate.y == 0 && blockCoordinate.z == 0)
            MapLoadStatic.Instance.RefreshExistingChunk(Lib.AddToVector(chunkCoordinate, -CHUNK_SIZE, -CHUNK_SIZE, -CHUNK_SIZE));
        else if (blockCoordinate.x == 0 && blockCoordinate.y == 0 && blockCoordinate.z == CHUNK_SIZE - 1)
            MapLoadStatic.Instance.RefreshExistingChunk(Lib.AddToVector(chunkCoordinate, -CHUNK_SIZE, -CHUNK_SIZE, CHUNK_SIZE));
        else if (blockCoordinate.x == 0 && blockCoordinate.y == CHUNK_SIZE - 1 && blockCoordinate.z == 0)
            MapLoadStatic.Instance.RefreshExistingChunk(Lib.AddToVector(chunkCoordinate, -CHUNK_SIZE, CHUNK_SIZE, -CHUNK_SIZE));
        else if (blockCoordinate.x == 0 && blockCoordinate.y == CHUNK_SIZE - 1 && blockCoordinate.z == CHUNK_SIZE - 1)
            MapLoadStatic.Instance.RefreshExistingChunk(Lib.AddToVector(chunkCoordinate, -CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE));
        else if (blockCoordinate.x == CHUNK_SIZE - 1 && blockCoordinate.y == 0 && blockCoordinate.z == 0)
            MapLoadStatic.Instance.RefreshExistingChunk(Lib.AddToVector(chunkCoordinate, CHUNK_SIZE, -CHUNK_SIZE, -CHUNK_SIZE));
        else if (blockCoordinate.x == CHUNK_SIZE - 1 && blockCoordinate.y == 0 && blockCoordinate.z == CHUNK_SIZE - 1)
            MapLoadStatic.Instance.RefreshExistingChunk(Lib.AddToVector(chunkCoordinate, CHUNK_SIZE, -CHUNK_SIZE, CHUNK_SIZE));
        else if (blockCoordinate.x == CHUNK_SIZE - 1 && blockCoordinate.y == CHUNK_SIZE - 1 && blockCoordinate.z == 0)
            MapLoadStatic.Instance.RefreshExistingChunk(Lib.AddToVector(chunkCoordinate, CHUNK_SIZE, CHUNK_SIZE, -CHUNK_SIZE));
        else if (blockCoordinate.x == CHUNK_SIZE - 1 && blockCoordinate.y == CHUNK_SIZE - 1 && blockCoordinate.z == CHUNK_SIZE - 1)
            MapLoadStatic.Instance.RefreshExistingChunk(Lib.AddToVector(chunkCoordinate, CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE));

        // Edge checks along X, Y, and Z axes
        if (blockCoordinate.x == 0 && blockCoordinate.y == 0)
            MapLoadStatic.Instance.RefreshExistingChunk(Lib.AddToVector(chunkCoordinate, -CHUNK_SIZE, -CHUNK_SIZE, 0));
        else if (blockCoordinate.x == 0 && blockCoordinate.z == 0)
            MapLoadStatic.Instance.RefreshExistingChunk(Lib.AddToVector(chunkCoordinate, -CHUNK_SIZE, 0, -CHUNK_SIZE));
        else if (blockCoordinate.y == 0 && blockCoordinate.z == 0)
            MapLoadStatic.Instance.RefreshExistingChunk(Lib.AddToVector(chunkCoordinate, 0, -CHUNK_SIZE, -CHUNK_SIZE));
        else if (blockCoordinate.x == CHUNK_SIZE - 1 && blockCoordinate.y == 0)
            MapLoadStatic.Instance.RefreshExistingChunk(Lib.AddToVector(chunkCoordinate, CHUNK_SIZE, -CHUNK_SIZE, 0));
        else if (blockCoordinate.x == CHUNK_SIZE - 1 && blockCoordinate.z == 0)
            MapLoadStatic.Instance.RefreshExistingChunk(Lib.AddToVector(chunkCoordinate, CHUNK_SIZE, 0, -CHUNK_SIZE));
        else if (blockCoordinate.y == CHUNK_SIZE - 1 && blockCoordinate.z == 0)
            MapLoadStatic.Instance.RefreshExistingChunk(Lib.AddToVector(chunkCoordinate, 0, CHUNK_SIZE, -CHUNK_SIZE));
        else if (blockCoordinate.x == 0 && blockCoordinate.y == CHUNK_SIZE - 1)
            MapLoadStatic.Instance.RefreshExistingChunk(Lib.AddToVector(chunkCoordinate, -CHUNK_SIZE, CHUNK_SIZE, 0));
        else if (blockCoordinate.x == 0 && blockCoordinate.z == CHUNK_SIZE - 1)
            MapLoadStatic.Instance.RefreshExistingChunk(Lib.AddToVector(chunkCoordinate, -CHUNK_SIZE, 0, CHUNK_SIZE));
        else if (blockCoordinate.y == 0 && blockCoordinate.z == CHUNK_SIZE - 1)
            MapLoadStatic.Instance.RefreshExistingChunk(Lib.AddToVector(chunkCoordinate, 0, -CHUNK_SIZE, CHUNK_SIZE));
        else if (blockCoordinate.x == CHUNK_SIZE - 1 && blockCoordinate.y == CHUNK_SIZE - 1)
            MapLoadStatic.Instance.RefreshExistingChunk(Lib.AddToVector(chunkCoordinate, CHUNK_SIZE, CHUNK_SIZE, 0));
        else if (blockCoordinate.x == CHUNK_SIZE - 1 && blockCoordinate.z == CHUNK_SIZE - 1)
            MapLoadStatic.Instance.RefreshExistingChunk(Lib.AddToVector(chunkCoordinate, CHUNK_SIZE, 0, CHUNK_SIZE));
        else if (blockCoordinate.y == CHUNK_SIZE - 1 && blockCoordinate.z == CHUNK_SIZE - 1)
            MapLoadStatic.Instance.RefreshExistingChunk(Lib.AddToVector(chunkCoordinate, 0, CHUNK_SIZE, CHUNK_SIZE));
 
    }

 
    public ChunkData GetChunk(Vector3Int coordinates)
    {
        try 
        { 
            if (World != null && World[coordinates.x, coordinates.y, coordinates.z] != ChunkData.Zero)
            {
                return World[coordinates.x, coordinates.y, coordinates.z];
            }
            return null;
        }
        catch (Exception ex)
        {
            Lib.Log("Error loading chunk: ", ex);
            return null;
        }
    }

 


    public static Vector3Int GetChunkCoordinate(Vector3 blockCoordinate) 
    {
        Vector3Int chunkPosition = new Vector3Int(
            (int)Mathf.Floor(blockCoordinate.x / CHUNK_SIZE) * CHUNK_SIZE, 
            (int)Mathf.Floor(blockCoordinate.y / CHUNK_SIZE) * CHUNK_SIZE, 
            (int)Mathf.Floor(blockCoordinate.z / CHUNK_SIZE) * CHUNK_SIZE);
        return chunkPosition;
    } 
    
    public static Vector3 GetBlockCoordinate(Vector3 blockCoordinate) 
    {
        Vector3Int chunkPosition = GetChunkCoordinate(blockCoordinate);

        Vector3 blockPositionInChunk = new Vector3(
            blockCoordinate.x - chunkPosition.x, 
            blockCoordinate.y - chunkPosition.y, 
            blockCoordinate.z - chunkPosition.z);

        return blockPositionInChunk;
    }
























 

    public void PrintChunks(int[,,] chunkBlocksArray)
    {
        int numLetters = 2; // Adjust this as needed

        string cube = "CHUNK VISUALISER\n\n";
        for (int y = 2; y >= 0; y--)
        {
            string plane = "";
            for (int z = CHUNK_SIZE -1; z >= 0; z--)
            {
                string row = "";
                for (int x = 0; x < CHUNK_SIZE; x++)
                {
                    int blockID = chunkBlocksArray[x, y, z];
                    if (blockID != 0)
                    { 
                        string blockStringID = BlockStatic.ConvertID(blockID);
                        string blockName = blockStringID.Length > numLetters ? blockStringID.Substring(0, numLetters) : blockStringID;
                        row += blockName.ToLower() + " ";
                    }
                    else
                    {
                        row += "@ ";
                    }
                }
                plane += row + "\n";
            }
            string layer = "Layer " + (y + 1) + "\n" + plane + "\n==============================================================\n\n";
            cube += layer;
        }
        Lib.Log(cube);
    }
}