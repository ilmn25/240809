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

public class WorldStatic : MonoBehaviour
{ 
    public static WorldStatic Instance { get; private set; }  
    
    public static Dictionary<int, List<ChunkData>> _loadedChunks;  

    private Dictionary<Vector3Int, ChunkData> _chunkCache = new Dictionary<Vector3Int, ChunkData>(); 
    private int MAX_CACHE_SIZE = 20;

    private static BinaryFormatter _bf = new BinaryFormatter(); 
    public static event Action PlayerChunkTraverse;
    public delegate void Vector3IntEvent(Vector3Int position); 
    public static event Vector3IntEvent MapUpdated;

    [HideInInspector] 
    public static Vector3Int _chunkPosition;
    [HideInInspector] 
    public static Vector3Int _chunkPositionPrevious = new Vector3Int(1,0,0);
    [HideInInspector] 
    public static Vector3Int _boolGridOrigin;
    [HideInInspector] 
    public static bool[,,] _boolGrid;
    [HideInInspector] 
    public static int CHUNKSIZE = 8; 
    [HideInInspector] 
    public static int CHUNKDEPTH = 25;
    private static int MAP_SIZE = 10; 
    public static int RENDER_DISTANCE = 5; 
    public static bool ALWAYS_REGENERATE = false;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Instance = this;
        _loadedChunks = new Dictionary<int, List<ChunkData>>(); 
 
        if (!File.Exists(getFilePath(0)) || ALWAYS_REGENERATE) GenerateRandomMapSave(); 
        _chunkPositionPrevious = GetChunkCoordinate(Game.Player.transform.position);
    }
          

    async void FixedUpdate()
    {
        _chunkPosition = GetChunkCoordinate(Game.Player.transform.position);
        if (_chunkPosition != _chunkPositionPrevious)
        {  
            PlayerChunkTraverse?.Invoke();
            _chunkPositionPrevious = _chunkPosition;
            await Task.Delay(80);
            (_boolGridOrigin, _boolGrid) = await Task.Run(() => 
                GenerateBoolMapAsync()
            ); 
        }
    }

 





    

    void OnApplicationQuit()
    {
        HandleSaveWorldFile(_loadedChunks[0] , 0);
    }
 
    public async void HandleSaveWorldFile(List<ChunkData> chunks, int yLevel)
    {
        EntityLoadStatic.Instance.SaveAll();
        EntityLoadDynamic.Instance.SaveAll();
        await Task.Delay(10);
        
        using (FileStream file = File.Create(getFilePath(yLevel)))
        {
            _bf.Serialize(file, chunks);
        }
    }

    private string getFilePath(int yLevel)
    {
        return $"{Game.DOWNLOAD_PATH}\\MAP{yLevel}.dat";
    }
    
    public void HandleLoadWorldFile(int yLevel)
    { 
        if (!_loadedChunks.ContainsKey(yLevel))
        {
            if (File.Exists(getFilePath(yLevel)))
            { 
                FileStream file = File.Open(getFilePath(yLevel), FileMode.Open);

                List<ChunkData> chunks = (List<ChunkData>)_bf.Deserialize(file);
                file.Close();
                _loadedChunks[yLevel] = chunks;
            } 
            else
            {
                Lib.Log("doesn't exist, load region fail" + yLevel); 
                _loadedChunks[yLevel] = new List<ChunkData>(); // if doesn't exist
            }
        }
    }








    public bool GetBoolInBoolMap(Vector3Int worldCoordinate)
    {
        if (_boolGrid == null) return true; 
        // Get the relative position in the bool map
        Vector3Int relativePosition = GetRelativePosition(worldCoordinate); 
        // Check if the relative position is within the bounds of the bool map
        if (relativePosition.x < 0 || relativePosition.x >= _boolGrid.GetLength(0) ||
            relativePosition.y < 0 || relativePosition.y >= _boolGrid.GetLength(1) ||
            relativePosition.z < 0 || relativePosition.z >= _boolGrid.GetLength(2))
        {
            // If out of bounds, return false
            return false;
        }

        // Access the bool map at the relative position
        return _boolGrid[relativePosition.x, relativePosition.y, relativePosition.z];
    }



    public async Task<(Vector3Int, bool[,,])> GenerateBoolMapAsync()
    {
        int chunkSpan = RENDER_DISTANCE * CHUNKSIZE;
        int _minXPath, _maxXPath, _minZPath, _maxZPath, _sizeXPath, _sizeZPath;
        List<Vector3Int> _chunkCoordinatesPathFind = new List<Vector3Int>();
        int _indexXPath, _indexZPath, _startXPath, _startYPath, _startZPath, _endXPath, _endYPath, _endZPath;
        bool[,,] _pathFindMap;
        ChunkData pathFindChunkData;

        _minXPath = _chunkPosition.x - chunkSpan;
        _maxXPath = _chunkPosition.x + chunkSpan;
        _minZPath = _chunkPosition.z - chunkSpan;
        _maxZPath = _chunkPosition.z + chunkSpan;

        _sizeXPath = (_maxXPath - _minXPath) / CHUNKSIZE + 1;
        _sizeZPath = (_maxZPath - _minZPath) / CHUNKSIZE + 1;

        _pathFindMap = new bool[_sizeXPath * CHUNKSIZE, CHUNKDEPTH, _sizeZPath * CHUNKSIZE];

        for (int x = _minXPath; x <= _maxXPath; x += CHUNKSIZE)
        {
            for (int z = _minZPath; z <= _maxZPath; z += CHUNKSIZE)
            {
                _chunkCoordinatesPathFind.Add(new Vector3Int(x, 0, z));
            }
        }

        foreach (var coordinates in _chunkCoordinatesPathFind)
        {
            pathFindChunkData = GetChunk(coordinates);
            if (pathFindChunkData != null)
            {
                _indexXPath = coordinates.x - _minXPath;
                _indexZPath = coordinates.z - _minZPath;

                for (int i = 0; i < CHUNKSIZE; i++)
                {
                    for (int j = 0; j < CHUNKDEPTH; j++)
                    {
                        for (int k = 0; k < CHUNKSIZE; k++)
                        {
                            _pathFindMap[_indexXPath + i, j, _indexZPath + k] = pathFindChunkData.Map[i, j, k] == 0;
                        }
                    }
                }

                foreach (var entity in pathFindChunkData.StaticEntity)
                {
                    if (entity.Type == EntityType.Static)
                    {
                        _startXPath = Mathf.FloorToInt(entity.Position.x);
                        _startYPath = Mathf.FloorToInt(entity.Position.y);
                        _startZPath = Mathf.FloorToInt(entity.Position.z);

                        _endXPath = _startXPath + entity.Bounds.x;
                        _endYPath = _startYPath + entity.Bounds.y;
                        _endZPath = _startZPath + entity.Bounds.z;

                        for (int x = _startXPath; x < _endXPath; x++)
                        {
                            for (int y = _startYPath; y < _endYPath; y++)
                            {
                                for (int z = _startZPath; z < _endZPath; z++)
                                {
                                    if (x >= 0 && x < CHUNKSIZE && y >= 0 && y < CHUNKDEPTH && z >= 0 && z < CHUNKSIZE)
                                    {
                                        _pathFindMap[_indexXPath + x, y, _indexZPath + z] = false;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Yield control back to the main thread to prevent lag spikes
            await Task.Delay(15);
        }

        return (new Vector3Int(_minXPath, 0, _minZPath), _pathFindMap);
    }


    public Vector3Int GetRelativePosition(Vector3Int coordinate)
    { 
        return new Vector3Int(
            coordinate.x - _boolGridOrigin.x,
            coordinate.y,
            coordinate.z - _boolGridOrigin.z
        );
    }

    public void SetBoolInMap(Vector3Int worldPosition, bool value)
    { 
        if (worldPosition.y >= CHUNKDEPTH) return;
        _boolGrid[worldPosition.x - _boolGridOrigin.x, 
            worldPosition.y, 
            worldPosition.z - _boolGridOrigin.z] = value;
        MapUpdated(worldPosition);
    }

    public void UpdateMap(Vector3Int worldCoordinate, Vector3Int chunkCoordinate, Vector3Int blockCoordinate, int blockID = 0)
    { 
        GetChunk(chunkCoordinate).Map[blockCoordinate.x, blockCoordinate.y, blockCoordinate.z] = blockID; 
        MapLoadStatic.Instance.RefreshExistingChunk(chunkCoordinate); //refresh on screen
            
        if (blockCoordinate.x == 0) 
        {
            MapLoadStatic.Instance.RefreshExistingChunk(Lib.AddToVector(chunkCoordinate, -CHUNKSIZE, 0, 0));
        }
        else if (blockCoordinate.x == CHUNKSIZE - 1) 
        {
            MapLoadStatic.Instance.RefreshExistingChunk(Lib.AddToVector(chunkCoordinate, CHUNKSIZE, 0, 0));
        }
        if (blockCoordinate.z == 0) 
        {
            MapLoadStatic.Instance.RefreshExistingChunk(Lib.AddToVector(chunkCoordinate, 0, 0, -CHUNKSIZE));
        }
        else if (blockCoordinate.z == CHUNKSIZE - 1) 
        {
            MapLoadStatic.Instance.RefreshExistingChunk(Lib.AddToVector(chunkCoordinate, 0, 0, CHUNKSIZE));
        }
        // corners
        if (blockCoordinate.x == 0 && blockCoordinate.z == 0) 
        {
            MapLoadStatic.Instance.RefreshExistingChunk(Lib.AddToVector(chunkCoordinate, -CHUNKSIZE, 0, -CHUNKSIZE));
        }
        else if (blockCoordinate.x == 0 && blockCoordinate.z == CHUNKSIZE - 1) 
        {
            MapLoadStatic.Instance.RefreshExistingChunk(Lib.AddToVector(chunkCoordinate, -CHUNKSIZE, 0, CHUNKSIZE));
        }
        else if (blockCoordinate.x == CHUNKSIZE - 1 && blockCoordinate.z == 0) 
        {
            MapLoadStatic.Instance.RefreshExistingChunk(Lib.AddToVector(chunkCoordinate, CHUNKSIZE, 0, -CHUNKSIZE));
        }
        else if (blockCoordinate.x == CHUNKSIZE - 1 && blockCoordinate.z == CHUNKSIZE - 1) 
        {
            MapLoadStatic.Instance.RefreshExistingChunk(Lib.AddToVector(chunkCoordinate, CHUNKSIZE, 0, CHUNKSIZE));
        } 
        SetBoolInMap(worldCoordinate, blockID == 0);
    }

 

    public ChunkData GetChunk(Vector3Int coordinates)
    {
        ChunkData loadChunkDataTemp;
        var _localChunkCache = new Dictionary<Vector3Int, ChunkData>(_chunkCache); // Local copy of the entire cache
        try 
        {
            if (!_localChunkCache.ContainsKey(coordinates))
            {  
                loadChunkDataTemp = null;
                HandleLoadWorldFile(0);
                foreach (var c in _loadedChunks[0])
                {
                    if (c.Coordinate.ToVector3Int() == coordinates)
                    {
                        loadChunkDataTemp = c;
                    }
                }
                
                
                
                
                if (loadChunkDataTemp != null) 
                {
                    if (_localChunkCache.Count >= MAX_CACHE_SIZE)
                    {
                        // Remove the first key from the local copy to avoid modification issues 
                        _localChunkCache.Remove(_localChunkCache.Keys.First());
                    }   
                    _localChunkCache[coordinates] = loadChunkDataTemp; 
                } 
            } 

            // Ensure the key exists before accessing it
            if (_localChunkCache.ContainsKey(coordinates))
            {
                // Update the original cache with the local copy
                _chunkCache = new Dictionary<Vector3Int, ChunkData>(_localChunkCache);
                return _localChunkCache[coordinates];
            }
            return null;
        }
        catch (Exception ex)
        {
            Lib.Log("error LoadChunk ", ex);
            return null;
        }
    }
 


    public static Vector3Int GetChunkCoordinate(Vector3 blockCoordinate) 
    {
        Vector3Int chunkPosition = new Vector3Int(
            (int)Mathf.Floor(blockCoordinate.x / CHUNKSIZE) * CHUNKSIZE, 0, 
            (int)Mathf.Floor(blockCoordinate.z / CHUNKSIZE) * CHUNKSIZE);
        return chunkPosition;
    } 
    
    public static Vector3 GetBlockCoordinate(Vector3 blockCoordinate) 
    {
        Vector3Int chunkPosition = GetChunkCoordinate(blockCoordinate);

        Vector3 blockPositionInChunk = new Vector3(
            blockCoordinate.x - chunkPosition.x, 
            blockCoordinate.y, 
            blockCoordinate.z - chunkPosition.z);

        return blockPositionInChunk;
    }






























    //! debug tools
    private void GenerateRandomMapSave()
    {
        int range = MAP_SIZE * CHUNKSIZE;
        // Generate and save the test chunks
        List<SerializableVector3Int> coordinatesList = new List<SerializableVector3Int>();

        for (int x = -range; x <= range; x += CHUNKSIZE)
        {
            for (int z = -range; z <= range; z += CHUNKSIZE)
            {
                coordinatesList.Add(new SerializableVector3Int(x, 0, z));
            }
        }

        Dictionary<int, List<ChunkData>> chunksByYLevel = new Dictionary<int, List<ChunkData>>();

        foreach (var coordinates in coordinatesList)
        {
            ChunkData chunkData = WorldGenStatic.Instance.GenerateTestChunk(coordinates.ToVector3Int());
            if (!chunksByYLevel.ContainsKey(coordinates.y)) //make new list in chunksByYLevel array if new y level
            {
                chunksByYLevel[coordinates.y] = new List<ChunkData>();
            }
            chunksByYLevel[coordinates.y].Add(chunkData); // add to list
             
        }

        foreach (var kvp in chunksByYLevel) //key is y, value is list for y chunks
        {
            HandleSaveWorldFile(kvp.Value, kvp.Key);
        }

    }

    public void PrintChunk(Vector3Int coordinates, int numLetters = 2)
    {
        if (!_loadedChunks.ContainsKey(0))
        {
            Lib.Log("No chunks loaded for this y level.");
            return;
        }

        ChunkData chunkData = null;
        foreach (var c in _loadedChunks[0])
        {
            if (c.Coordinate.ToVector3Int() == coordinates)
            {
                chunkData = c;
                break;
            }
        }

        if (chunkData == null)
        {
            Lib.Log("Chunk not found.");
            return;
        }

        string cube = coordinates+" CHUNK VISUALISER\n\n";
        for (int y = chunkData.Depth - 1; y >= 0; y--)
        {
            string plane = "";
            for (int z = chunkData.Size - 1; z >= 0; z--)
            {
                string row = "";
                for (int x = 0; x < chunkData.Size; x++)
                {
                    string blockID = BlockStatic.ConvertID(chunkData.Map[x, y, z]);
                    if (!string.IsNullOrEmpty(blockID))
                    {
                        string blockName = blockID.Length > numLetters ? blockID.Substring(0, numLetters) : blockID;
                        row += blockName.ToLower() + " ";
                    }
                    else
                    {
                        row += "@ ";
                    }
                }
                plane += row + "\n";
            }
            cube += "Layer " + (y + 1) + "\n" + plane + "\n==============================================================\n\n";
        }
        Lib.Log(cube);
    }

    public void PrintChunks(int[,,] chunkBlocksArray)
    {
        int sizeX = chunkBlocksArray.GetLength(0);
        int sizeY = chunkBlocksArray.GetLength(1);
        int sizeZ = chunkBlocksArray.GetLength(2);
        int numLetters = 2; // Adjust this as needed

        string cube = "CHUNK VISUALISER\n\n";
        for (int y = sizeY - 1; y >= 0; y--)
        {
            string plane = "";
            for (int z = sizeZ - 1; z >= 0; z--)
            {
                string row = "";
                for (int x = 0; x < sizeX; x++)
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
            Lib.Log(layer);
            cube += layer;
        }
        Lib.Log(cube);
    }


}