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

public class ChunkSystem : MonoBehaviour
{ 
    public static Dictionary<int, List<Chunk>> _loadedChunks;  

    Dictionary<Vector3Int, Chunk> _chunkCache = new Dictionary<Vector3Int, Chunk>(); 
    private int MAX_CACHE_SIZE = 20;

    private GameObject _player; 
    private BinaryFormatter _bf = new BinaryFormatter();
    private MapLoadSystem _mapLoadSystem;
    private EntityLoadSystem _entityLoadSystem;
    private ChunkGenerationSystem _chunkGenerationSystem;
    public static event Action PlayerChunkPositionUpdate;

    [HideInInspector] 
    public static Vector3Int _chunkPosition;
    [HideInInspector] 
    public static Vector3Int _chunkPositionPrevious = new Vector3Int(1,0,0);
    [HideInInspector] 
    public static Vector3Int _boolGridOrigin;
    [HideInInspector] 
    public static bool[,,] _boolGrid;
    [HideInInspector] 
    public static int CHUNKSIZE = 10; 
    [HideInInspector] 
    public static int CHUNKDEPTH = 35;

    public int RENDER_DISTANCE = 4; 
    public bool ALWAYS_REGENERATE = false;
 
    void Start()
    {
        _player = GameObject.Find("player"); 
        _mapLoadSystem = transform.Find("map_system").GetComponent<MapLoadSystem>();
        _entityLoadSystem = transform.Find("entity_system").GetComponent<EntityLoadSystem>();
        _chunkGenerationSystem = GetComponent<ChunkGenerationSystem>();
        _loadedChunks = new Dictionary<int, List<Chunk>>(); 

        //TODO 
        string downloadsPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";
        string filePath = $"{downloadsPath}\\chunks_{0}.dat";
        if (!File.Exists(filePath) || ALWAYS_REGENERATE)
        {
            GenerateRandomMapSave(); 
        }
        _chunkPositionPrevious = GetChunkCoordinate(_player.transform.position);
    }
          

    async void FixedUpdate()
    {
        _chunkPosition = GetChunkCoordinate(_player.transform.position);
        if (_chunkPosition != _chunkPositionPrevious)
        { 
            PlayerChunkPositionUpdate?.Invoke();
            _chunkPositionPrevious = _chunkPosition;
            await Task.Delay(80);
            (_boolGridOrigin, _boolGrid) = await Task.Run(() => 
                GenerateBoolMapAsync()
            ); 
        }
    }

 





    

    void OnApplicationQuit()
    {
        SaveChunks(_loadedChunks[0] , 0);
    }
 
    public async void SaveChunks(List<Chunk> chunks, int yLevel)
    {
        _entityLoadSystem.SaveAllEntities();
        await Task.Delay(10);
        string downloadsPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";
        string filePath = $"{downloadsPath}\\chunks_{yLevel}.dat"; 
        
        using (FileStream file = File.Create(filePath))
        {
            _bf.Serialize(file, chunks);
        }
    }

    
    public void HandleLoadRegion(int yLevel)
    { 
        if (!_loadedChunks.ContainsKey(yLevel))
        {
            string downloadsPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";
            string filePath = $"{downloadsPath}\\chunks_{yLevel}.dat";
            if (File.Exists(filePath))
            { 
                FileStream file = File.Open(filePath, FileMode.Open);

                List<Chunk> chunks = (List<Chunk>)_bf.Deserialize(file);
                file.Close();
                _loadedChunks[yLevel] = chunks;
            } 
            else
            {
                CustomLibrary.Log("doesn't exist, load region fail" + yLevel); 
                _loadedChunks[yLevel] = new List<Chunk>(); // if doesn't exist
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
        Chunk _pathFindChunk;

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
            _pathFindChunk = LoadChunk(coordinates);
            if (_pathFindChunk != null)
            {
                _indexXPath = coordinates.x - _minXPath;
                _indexZPath = coordinates.z - _minZPath;

                for (int i = 0; i < CHUNKSIZE; i++)
                {
                    for (int j = 0; j < CHUNKDEPTH; j++)
                    {
                        for (int k = 0; k < CHUNKSIZE; k++)
                        {
                            _pathFindMap[_indexXPath + i, j, _indexZPath + k] = _pathFindChunk.Map[i, j, k] == 0;
                        }
                    }
                }

                foreach (var entity in _pathFindChunk.Entity)
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












    Chunk _targetChunkTemp;
    int[,,] _targetChunk;
    bool[,] _frontFace;
    bool[,] _backFace;
    bool[,] _leftFace;
    bool[,] _rightFace;
    bool[,] _corner;
    public (int[,,], bool[,], bool[,], bool[,], bool[,], bool[,]) LoadChunkMap(Vector3Int coordinates)
    {
        _targetChunk = null;
        _frontFace = null;
        _backFace = null;
        _leftFace = null;
        _rightFace = null;
        _corner = new bool[4, CHUNKDEPTH];

        _targetChunkTemp = LoadChunk(new Vector3Int(coordinates.x, coordinates.y, coordinates.z - CHUNKSIZE));
        if (_targetChunkTemp != null)
        {
            _frontFace = new bool[CHUNKDEPTH, CHUNKSIZE];
            for (int y = 0; y < CHUNKDEPTH; y++)
            {
                for (int x = 0; x < CHUNKSIZE; x++)
                {
                    _frontFace[y, x] = !IsZero(_targetChunkTemp.Map[x, y, CHUNKSIZE - 1]);
                }
            }
        } 

        _targetChunkTemp = LoadChunk(new Vector3Int(coordinates.x, coordinates.y, coordinates.z + CHUNKSIZE));
        if (_targetChunkTemp != null)
        {
            _backFace = new bool[CHUNKDEPTH, CHUNKSIZE];
            for (int y = 0; y < CHUNKDEPTH; y++)
            {
                for (int x = 0; x < CHUNKSIZE; x++)
                {
                    _backFace[y, x] = !IsZero(_targetChunkTemp.Map[x, y, 0]);
                }
            }
        } 

        _targetChunkTemp = LoadChunk(new Vector3Int(coordinates.x - CHUNKSIZE, coordinates.y, coordinates.z));
        if (_targetChunkTemp != null)
        {
            _leftFace = new bool[CHUNKDEPTH, CHUNKSIZE];
            for (int y = 0; y < CHUNKDEPTH; y++)
            {
                for (int z = 0; z < CHUNKSIZE; z++)
                {
                    _leftFace[y, z] = !IsZero(_targetChunkTemp.Map[CHUNKSIZE - 1, y, z]);
                }
            }   
        }  

        _targetChunkTemp = LoadChunk(new Vector3Int(coordinates.x + CHUNKSIZE, coordinates.y, coordinates.z));
        if (_targetChunkTemp != null)
        {
            _rightFace = new bool[CHUNKDEPTH, CHUNKSIZE];
            for (int y = 0; y < CHUNKDEPTH; y++)
            {
                for (int z = 0; z < CHUNKSIZE; z++)
                {
                    _rightFace[y, z] = !IsZero(_targetChunkTemp.Map[0, y, z]);
                }
            }
        } 
        
        _targetChunkTemp = LoadChunk(new Vector3Int(coordinates.x - CHUNKSIZE, coordinates.y, coordinates.z + CHUNKSIZE));
        if (_targetChunkTemp != null)
        {
            for (int y = 0; y < CHUNKDEPTH; y++)
            {
                _corner[0, y] = !IsZero(_targetChunkTemp.Map[CHUNKSIZE - 1, y, 0]);
            }
        }

        _targetChunkTemp = LoadChunk(new Vector3Int(coordinates.x + CHUNKSIZE, coordinates.y, coordinates.z + CHUNKSIZE));
        if (_targetChunkTemp != null)
        {
            for (int y = 0; y < CHUNKDEPTH; y++)
            {
                _corner[1, y] = !IsZero(_targetChunkTemp.Map[0, y, 0]);
            }
        }

        _targetChunkTemp = LoadChunk(new Vector3Int(coordinates.x - CHUNKSIZE, coordinates.y, coordinates.z - CHUNKSIZE));
        if (_targetChunkTemp != null)
        {
            for (int y = 0; y < CHUNKDEPTH; y++)
            {
                _corner[2, y] = !IsZero(_targetChunkTemp.Map[CHUNKSIZE - 1, y, CHUNKSIZE - 1]);
            }
        }

        _targetChunkTemp = LoadChunk(new Vector3Int(coordinates.x + CHUNKSIZE, coordinates.y, coordinates.z - CHUNKSIZE));
        if (_targetChunkTemp != null)
        {
            for (int y = 0; y < CHUNKDEPTH; y++)
            {
                _corner[3, y] = !IsZero(_targetChunkTemp.Map[0, y, CHUNKSIZE - 1]);
            } 
        }

        Chunk _chunkTemp = LoadChunk(coordinates);
        _targetChunk = _chunkTemp == null? null : _chunkTemp.Map;
        return (_targetChunk, _frontFace, _backFace, _leftFace, _rightFace, _corner);
    }

    private bool IsZero(int ID)
    {
        return ID == 0;
    }

    public void UpdateBlock(Vector3Int chunkCoordinate, Vector3Int blockCoordinate, int blockID = 0) 
    { 
        LoadChunk(chunkCoordinate).Map[blockCoordinate.x, blockCoordinate.y, blockCoordinate.z] = blockID; 
        _mapLoadSystem.RefreshExistingChunk(chunkCoordinate); //refresh on screen
            
        if (blockCoordinate.x == 0) 
        {
            _mapLoadSystem.RefreshExistingChunk(CustomLibrary.AddToVector(chunkCoordinate, -CHUNKSIZE, 0, 0));
        }
        else if (blockCoordinate.x == CHUNKSIZE - 1) 
        {
            _mapLoadSystem.RefreshExistingChunk(CustomLibrary.AddToVector(chunkCoordinate, CHUNKSIZE, 0, 0));
        }
        if (blockCoordinate.z == 0) 
        {
            _mapLoadSystem.RefreshExistingChunk(CustomLibrary.AddToVector(chunkCoordinate, 0, 0, -CHUNKSIZE));
        }
        else if (blockCoordinate.z == CHUNKSIZE - 1) 
        {
            _mapLoadSystem.RefreshExistingChunk(CustomLibrary.AddToVector(chunkCoordinate, 0, 0, CHUNKSIZE));
        }
        // corners
        if (blockCoordinate.x == 0 && blockCoordinate.z == 0) 
        {
            _mapLoadSystem.RefreshExistingChunk(CustomLibrary.AddToVector(chunkCoordinate, -CHUNKSIZE, 0, -CHUNKSIZE));
        }
        else if (blockCoordinate.x == 0 && blockCoordinate.z == CHUNKSIZE - 1) 
        {
            _mapLoadSystem.RefreshExistingChunk(CustomLibrary.AddToVector(chunkCoordinate, -CHUNKSIZE, 0, CHUNKSIZE));
        }
        else if (blockCoordinate.x == CHUNKSIZE - 1 && blockCoordinate.z == 0) 
        {
            _mapLoadSystem.RefreshExistingChunk(CustomLibrary.AddToVector(chunkCoordinate, CHUNKSIZE, 0, -CHUNKSIZE));
        }
        else if (blockCoordinate.x == CHUNKSIZE - 1 && blockCoordinate.z == CHUNKSIZE - 1) 
        {
            _mapLoadSystem.RefreshExistingChunk(CustomLibrary.AddToVector(chunkCoordinate, CHUNKSIZE, 0, CHUNKSIZE));
        } 
    }

 

    public Chunk LoadChunk(Vector3Int coordinates)
    {
        Chunk _loadChunkTemp;
        var _localChunkCache = new Dictionary<Vector3Int, Chunk>(_chunkCache); // Local copy of the entire cache
        try 
        {
            if (!_localChunkCache.ContainsKey(coordinates))
            {  
                _loadChunkTemp = LoadChunkFromSource(coordinates);
                if (_loadChunkTemp != null) 
                {
                    if (_localChunkCache.Count >= MAX_CACHE_SIZE)
                    {
                        // Remove the first key from the local copy to avoid modification issues 
                        _localChunkCache.Remove(_localChunkCache.Keys.First());
                    }   
                    _localChunkCache[coordinates] = _loadChunkTemp; 
                } 
            } 

            // Ensure the key exists before accessing it
            if (_localChunkCache.ContainsKey(coordinates))
            {
                // Update the original cache with the local copy
                _chunkCache = new Dictionary<Vector3Int, Chunk>(_localChunkCache);
                return _localChunkCache[coordinates];
            }
            return null;
        }
        catch (Exception ex)
        {
            CustomLibrary.Log("error LoadChunk ", ex);
            return null;
        }
    }

    private Chunk LoadChunkFromSource(Vector3Int coordinates)
    {
        HandleLoadRegion(0);

        foreach (var c in _loadedChunks[0])
        {
            if (c.Coordinate.ToVector3Int() == coordinates)
            {
                return c;
            }
        }
        return null;
    }









 

    public int GetBlockInChunk(Vector3Int chunkCoordinate, Vector3Int blockCoordinate) //0 = empty, 1 = block, error = out of bounds
    {
        try
        {
            if (blockCoordinate.y >= 0 && blockCoordinate.y < CHUNKDEPTH)
            {
                var chunk = LoadChunk(chunkCoordinate);
                if (chunk != null)
                {
                    return chunk.Map[blockCoordinate.x, blockCoordinate.y, blockCoordinate.z];
                }
            }
            return 0;
        }
        catch
        {
            CustomLibrary.Log("error in isblocknull");
            return 0;
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
        int range = 8 * CHUNKSIZE;
        // Generate and save the test chunks
        List<SerializableVector3Int> coordinatesList = new List<SerializableVector3Int>();

        for (int x = -range; x <= range; x += CHUNKSIZE)
        {
            for (int z = -range; z <= range; z += CHUNKSIZE)
            {
                coordinatesList.Add(new SerializableVector3Int(x, 0, z));
            }
        }

        Dictionary<int, List<Chunk>> chunksByYLevel = new Dictionary<int, List<Chunk>>();

        foreach (var coordinates in coordinatesList)
        {
            Chunk chunk = _chunkGenerationSystem.GenerateTestChunk(coordinates.ToVector3Int());
            if (!chunksByYLevel.ContainsKey(coordinates.y)) //make new list in chunksByYLevel array if new y level
            {
                chunksByYLevel[coordinates.y] = new List<Chunk>();
            }
            chunksByYLevel[coordinates.y].Add(chunk); // add to list
             
        }

        foreach (var kvp in chunksByYLevel) //key is y, value is list for y chunks
        {
            SaveChunks(kvp.Value, kvp.Key);
        }

    }

    public void PrintChunk(Vector3Int coordinates, int numLetters = 2)
    {
        if (!_loadedChunks.ContainsKey(0))
        {
            CustomLibrary.Log("No chunks loaded for this y level.");
            return;
        }

        Chunk chunk = null;
        foreach (var c in _loadedChunks[0])
        {
            if (c.Coordinate.ToVector3Int() == coordinates)
            {
                chunk = c;
                break;
            }
        }

        if (chunk == null)
        {
            CustomLibrary.Log("Chunk not found.");
            return;
        }

        string cube = coordinates+" CHUNK VISUALISER\n\n";
        for (int y = chunk.Depth - 1; y >= 0; y--)
        {
            string plane = "";
            for (int z = chunk.Size - 1; z >= 0; z--)
            {
                string row = "";
                for (int x = 0; x < chunk.Size; x++)
                {
                    string blockID = BlockSystem.GetStringIDByID(chunk.Map[x, y, z]);
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
        CustomLibrary.Log(cube);
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
                        string blockStringID = BlockSystem.GetStringIDByID(blockID);
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
            CustomLibrary.Log(layer);
            cube += layer;
        }
        CustomLibrary.Log(cube);
    }


}














 
[System.Serializable] 
public class Chunk
{
    public int[,,] Map { get; private set; }
    public int Size { get; private set; } = ChunkSystem.CHUNKSIZE;
    public int Depth { get; private set; } = ChunkSystem.CHUNKDEPTH;
    public SerializableVector3Int Coordinate { get; private set; }
    public List<Entity> Entity { get; private set; }

    public Chunk(SerializableVector3Int coordinate)
    {
        Coordinate = coordinate;
        Map = new int[Size, Depth, Size];
        Entity = new List<Entity>();
    }
}
  
[System.Serializable]
public class SerializableVector3Int
{
    public int x;
    public int y;
    public int z;

    public SerializableVector3Int(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public SerializableVector3Int(Vector3Int vector)
    {
        this.x = vector.x;
        this.y = vector.y;
        this.z = vector.z;
    }

    public Vector3Int ToVector3Int()
    {
        return new Vector3Int(x, y, z);
    } 
}

  
[System.Serializable]
public class SerializableVector3
{
    public float x;
    public float y;
    public float z;

    public SerializableVector3(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public SerializableVector3(Vector3 vector)
    {
        this.x = vector.x;
        this.y = vector.y;
        this.z = vector.z;
    }
 
    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }
}
