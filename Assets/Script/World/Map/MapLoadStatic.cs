using System.Collections.Generic;
using UnityEngine; 
using System.Threading.Tasks;
using System.Threading;
using System;
// using System.Collections;
using Unity.VisualScripting;
using Unity.Jobs;
using Unity.Collections;
using UnityEngine.Profiling;
// using Unity.Burst;

public class MapLoadStatic : MonoBehaviour
         
{
    public static MapLoadStatic Instance { get; private set; }  
  
    
    public void Awake()
    {   
        Instance = this;
        WorldStatic.PlayerChunkTraverse += HandleChunkMapTraverse;
        _chunkSize = WorldStatic.CHUNKSIZE;
        _chunkDepth = WorldStatic.CHUNKDEPTH;

        _tileSize = 16;
        _tilesPerRow = 12;
        _colx = new int[] {0, 16, 32, 48, 64, 80, 96, 112, 128, 144, 160, 176};
        _rowy = new int[] {112, 96, 80, 64, 48, 32, 16, 0}; 
        _nullFace = new bool[_chunkDepth, _chunkSize]; 
        for (int i = 0; i < _chunkDepth; i++)
        {
            for (int j = 0; j < _chunkSize; j++)
            {
                _nullFace[i, j] = false; 
            }
        }  
    } 

    async void Start()
    {
        await Task.Delay(50);
        HandleChunkMapTraverse();
    }















    
    public void RefreshExistingChunk(Vector3Int chunkCoordinates)
    { 
        _ = LoadChunksOntoScreenAsync(chunkCoordinates, true);
    }
 

    Vector3 traverseCheckPosition;
    List<Vector3Int> destroyList = new List<Vector3Int>();
    void HandleChunkMapTraverse()
    { 

        foreach (var kvp in _activeChunks)
        {
            traverseCheckPosition = kvp.Key;
            if (traverseCheckPosition.x > WorldStatic._chunkPosition.x + WorldStatic.RENDER_DISTANCE * _chunkSize || traverseCheckPosition.x < WorldStatic._chunkPosition.x - WorldStatic.RENDER_DISTANCE * _chunkSize
                || traverseCheckPosition.z > WorldStatic._chunkPosition.z + WorldStatic.RENDER_DISTANCE * _chunkSize || traverseCheckPosition.z < WorldStatic._chunkPosition.z - WorldStatic.RENDER_DISTANCE * _chunkSize)
            {
                Destroy(kvp.Value);
                destroyList.Add(kvp.Key);
            }
        }

        // Remove the keys after iteration to avoid modifying the collection while iterating
        foreach (var key in destroyList)
        {
            _activeChunks.Remove(key);
        }
        destroyList.Clear();

        // Collect chunk coordinates within render distance
        for (int x = -WorldStatic.RENDER_DISTANCE; x <= WorldStatic.RENDER_DISTANCE; x++)
        {
            for (int z = -WorldStatic.RENDER_DISTANCE; z <= WorldStatic.RENDER_DISTANCE; z++)
            {
                Vector3Int traverseCheckPosition = new Vector3Int(
                    WorldStatic._chunkPosition.x + x * _chunkSize,
                    0,
                    WorldStatic._chunkPosition.z + z * _chunkSize
                );
                if (!_activeChunks.ContainsKey(traverseCheckPosition)) _ = LoadChunksOntoScreenAsync(traverseCheckPosition);
            }
        } 
    }

    private Dictionary<Vector3Int, GameObject> _activeChunks = new Dictionary<Vector3Int, GameObject>();
    private SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1); 
    private async Task LoadChunksOntoScreenAsync(Vector3Int chunkCoord, bool replace = false)
    { 
        await _semaphoreSlim.WaitAsync();
        try
        { 
            if (replace || !_activeChunks.ContainsKey(chunkCoord))
            {
                _chunkCoordinate = chunkCoord;
                (_chunkMap, _frontFace, _backFace, _leftFace, _rightFace, _corner) = LoadChunkMap(chunkCoord, WorldStatic.Instance);
                if (_chunkMap != null)
                {
                    await Task.Run(() => LoadMeshMath()); 
                    await Task.Delay(10);
                    LoadMeshObject(replace);
                }  
            }
        }
        catch (Exception ex)
        {
            _semaphoreSlim.Release();
            throw new Exception("An exception occurred while queuing the task.", ex);
        } 
        finally
        {
            _semaphoreSlim.Release();
        }
    }
 

 
    //! SUBSYSTEMS BELOW

    Mesh _mesh; 
    GameObject _meshObject;
    MeshRenderer _meshRenderer;
    MapCullInst _mapCullInst;
    private void LoadMeshObject(bool replace = false)
    {
        _mesh = new Mesh();
        _mesh.SetVertices(_vertices);
        _mesh.SetTriangles(_triangles, 0);
        _mesh.SetUVs(0, _uvs);
        _mesh.SetNormals(_normals); 
 
        if (!replace)
        {
            _meshObject = new($"{_chunkCoordinate.x}_{_chunkCoordinate.z}");
            _meshObject.layer = LayerMask.NameToLayer("Collision");
            _meshObject.transform.parent = transform;  
            _meshObject.transform.position = _chunkCoordinate; 

            _meshObject.AddComponent<MeshFilter>();
            _meshRenderer = _meshObject.AddComponent<MeshRenderer>();  
            _meshRenderer.material = BlockStatic._meshMaterial; 

            _mapCullInst = _meshObject.AddComponent<MapCullInst>();  
            _mapCullInst._chunkMap = _chunkMap;
            _mapCullInst._meshData = _mesh;
            _mapCullInst._verticesShadow = _verticesShadow;
            _activeChunks.Add(_chunkCoordinate, _meshObject);
        } 
        else 
        {
            _meshObject = _activeChunks[_chunkCoordinate];
            _meshRenderer = _meshObject.GetComponent<MeshRenderer>(); 
            _meshRenderer.material = BlockStatic._meshMaterial; 

            _mapCullInst = _meshObject.GetComponent<MapCullInst>();  
            _mapCullInst._chunkMap = _chunkMap;
            _mapCullInst._meshData = _mesh;
            _mapCullInst._verticesShadow = _verticesShadow;
            _mapCullInst.HandleAssignment(); 
        } 
    }
 



    // const
    private int _chunkSize;
    private int _chunkDepth;  
    private int _tileSize;
    private int _tilesPerRow;
    private int[] _colx;
    private int[] _rowy; 
    private bool[,] _nullFace; 

    // input
    public Vector3Int _chunkCoordinate; 
    public int[,,] _chunkMap; 
    private bool[,] _frontFace;
    private bool[,] _backFace;
    private bool[,] _leftFace;
    private bool[,] _rightFace; 
    private bool[,] _corner; 

    // output
    private List<Vector3> _vertices;
    private List<Vector3> _verticesShadow;
    private List<int> _triangles;
    private List<Vector2> _uvs; 
    private List<Vector3> _normals;    

    private void LoadMeshMath()
    { 
        try
        {   
            NativeHashMap<int, Rect> _textureRectDictionary = new NativeHashMap<int, Rect>(BlockStatic._textureRectDictionary.Count, Allocator.TempJob);
            foreach (var kvp in BlockStatic._textureRectDictionary)
            {
                _textureRectDictionary[kvp.Key] = kvp.Value;
            }

            // Initialize local temp arrays 
            MeshMathJob job = new MeshMathJob
            {
                // const
                _chunkSize = _chunkSize,
                _chunkDepth = _chunkDepth, 
                _tileSize = _tileSize,
                _tilesPerRow = _tilesPerRow, 
                _colx = new NativeArray<int>(_colx, Allocator.TempJob),
                _rowy = new NativeArray<int>(_rowy, Allocator.TempJob),  
                _textureRectDictionary = _textureRectDictionary,
                _textureAtlasWidth = BlockStatic._textureAtlasWidth,
                _textureAtlasHeight = BlockStatic._textureAtlasHeight, 

                // input
                _chunkMap = new NativeArray3D<int>(_chunkMap, Allocator.TempJob),
                _frontFace = InitializeFace(_frontFace, Allocator.TempJob),
                _backFace = InitializeFace(_backFace, Allocator.TempJob),
                _leftFace = InitializeFace(_leftFace, Allocator.TempJob),
                _rightFace = InitializeFace(_rightFace, Allocator.TempJob),
                _corner = InitializeFace(_corner, Allocator.TempJob),

                // output
                _vertices = new NativeList<Vector3>(Allocator.TempJob),
                _verticesShadow = new NativeList<Vector3>(Allocator.TempJob),
                _triangles = new NativeList<int>(Allocator.TempJob),
                _uvs = new NativeList<Vector2>(Allocator.TempJob),
                _normals = new NativeList<Vector3>(Allocator.TempJob),

                // local temp
                _blockData = new NativeArray<int>(6, Allocator.TempJob),
                _faceVertices = new NativeArray<Vector3>(4, Allocator.TempJob),
                _faceVerticesShadow = new NativeArray<Vector3>(4, Allocator.TempJob),
                _spriteUVs = new NativeArray<Vector2>(4, Allocator.TempJob)
            };

            // Schedule the job
            JobHandle jobHandle = job.Schedule();

            jobHandle.Complete();
            
            _vertices = new List<Vector3>(job._vertices.ToArray());
            _verticesShadow = new List<Vector3>(job._verticesShadow.ToArray());
            _triangles = new List<int>(job._triangles.ToArray());
            _uvs = new List<Vector2>(job._uvs.ToArray());
            _normals = new List<Vector3>(job._normals.ToArray());   

            // Dispose of the native arrays and lists 
            job._vertices.Dispose();
            job._verticesShadow.Dispose();
            job._triangles.Dispose();
            job._uvs.Dispose();
            job._normals.Dispose(); 

            job._textureRectDictionary.Dispose();
        }
        catch (Exception ex)
        {
            Debug.LogError($"An exception occurred in MeshMathJob: {ex.Message}");
        } 
    }
        
    private NativeArray2D<bool> InitializeFace(bool[,] face, Allocator allocator)
    {
        if (face != null)
        {
            return new NativeArray2D<bool>(face, allocator) { IsValid = true };
        }
        else
        {
            return new NativeArray2D<bool>(_nullFace, allocator) { IsValid = false };
        }
    }
 

    // [BurstCompile(CompileSynchronously = true)]
    public struct MeshMathJob : IJob
    {
        // const
        [DeallocateOnJobCompletion]
        public int _chunkSize;
        [DeallocateOnJobCompletion]
        public int _chunkDepth;   
        [DeallocateOnJobCompletion]
        public int _tileSize;
        [DeallocateOnJobCompletion]
        public int _tilesPerRow;
        [DeallocateOnJobCompletion]
        public NativeArray<int> _colx;
        [DeallocateOnJobCompletion]
        public NativeArray<int> _rowy;    

        public NativeHashMap<int, Rect> _textureRectDictionary;
        [DeallocateOnJobCompletion]
        public int _textureAtlasWidth;
        [DeallocateOnJobCompletion]
        public int _textureAtlasHeight; 

        // input 
        [DeallocateOnJobCompletion]
        public NativeArray3D<int> _chunkMap; 
        [DeallocateOnJobCompletion]
        public NativeArray2D<bool> _frontFace;
        [DeallocateOnJobCompletion]
        public NativeArray2D<bool> _backFace;
        [DeallocateOnJobCompletion]
        public NativeArray2D<bool> _leftFace;
        [DeallocateOnJobCompletion]
        public NativeArray2D<bool> _rightFace; 
        [DeallocateOnJobCompletion]
        public NativeArray2D<bool> _corner;

        // output
        public NativeList<Vector3> _vertices;
        public NativeList<Vector3> _verticesShadow;
        public NativeList<int> _triangles;
        public NativeList<Vector2> _uvs;
        public NativeList<Vector3> _normals; 

        // local temp
        [DeallocateOnJobCompletion]
        private int _blockID;
        [DeallocateOnJobCompletion] 
        private Vector3 _blockPosition;
        [DeallocateOnJobCompletion]
        public NativeArray<int> _blockData;
        [DeallocateOnJobCompletion]
        public NativeArray<Vector3> _faceVertices;
        [DeallocateOnJobCompletion]
        public NativeArray<Vector3> _faceVerticesShadow;
        [DeallocateOnJobCompletion] 
        private Vector3 _normal;
        [DeallocateOnJobCompletion]
        public NativeArray<Vector2> _spriteUVs; 
        [DeallocateOnJobCompletion]
        private int spriteNumber;
        [DeallocateOnJobCompletion]
        private int edgeValue;
        [DeallocateOnJobCompletion]
        private int cornerValue; 
        [DeallocateOnJobCompletion]
        private bool isBackEdge;
        [DeallocateOnJobCompletion]
        private bool isRightEdge;
        [DeallocateOnJobCompletion]
        private bool isFrontEdge;
        [DeallocateOnJobCompletion]
        private bool isLeftEdge;
        [DeallocateOnJobCompletion]
        private bool isTop;
        [DeallocateOnJobCompletion] 
        private bool isBottem;
        [DeallocateOnJobCompletion] 
        private Rect _textureRect;

        
        // private int GetIndex2D(int y, int x)
        // {
        //     return y * (_chunkSize - 1) + x;
        // }

        // private int GetIndex3D(int x, int y, int z)
        // {
        //     return x * (_chunkDepth -1) * (_chunkSize -1) + y * (_chunkSize - 1) + z;
        // }

        public void Execute()
        {   
            // int threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
            // UnityEngine.Debug.Log(threadId == 1 ? "Main thread" : $"Worker thread {threadId}");
            for (int z = 0; z < _chunkSize; z++)
            {
                for (int x = 0; x < _chunkSize; x++)
                {
                    for (int y = 0; y < _chunkDepth; y++)
                    {
                        _blockID = _chunkMap[x, y, z];

                        if (_blockID != 0)
                        {
                            HandleBlockProcessData(x, y, z);    
                            _blockPosition = new Vector3(x, y, z);

                            //! Check if the block ID is already in the list, and used to calculate textureatlas size needed  
                            if (_blockData[0] != -1) HandleMeshFace(1, _blockData[0]);//top
                            if (_blockData[1] != -1)
                            {
                                if (_blockData[5] == 1 || _blockData[5] == 2) HandleMeshFace(4, _blockData[1]);//front
                                if (_blockData[5] == 1 || _blockData[5] == 3) HandleMeshFace(3, _blockData[1]);//back
                            }

                            if (_blockData[2] != -1)
                            {
                                if (_blockData[3] == 1 || _blockData[3] == 2) HandleMeshFace(5, _blockData[2]);//left
                                if (_blockData[3] == 1 || _blockData[3] == 3) HandleMeshFace(6, _blockData[2]);//right
                            }
                            if (_blockData[4] == 1) HandleMeshFace(2, 33);
                                
                        }
                    }
                }
            } 
 
        }

        void HandleMeshFace(int direction, int textureIndex)
        { 
            int vertexIndex = _vertices.Length; 
            _normal = Vector3.zero;

            if (direction == 1) // top
            {
                _faceVertices[0] = _blockPosition + new Vector3(0, 1, 0);
                _faceVertices[1] = _blockPosition + new Vector3(1, 1, 0);
                _faceVertices[2] = _blockPosition + new Vector3(1, 1, 1);
                _faceVertices[3] = _blockPosition + new Vector3(0, 1, 1);

                _faceVerticesShadow[0] = _blockPosition + new Vector3(-0.03f, 1, -0.03f);
                _faceVerticesShadow[1] = _blockPosition + new Vector3(1.03f, 1, -0.03f);
                _faceVerticesShadow[2] = _blockPosition + new Vector3(1.03f, 1, 1.03f);
                _faceVerticesShadow[3] = _blockPosition + new Vector3(-0.03f, 1, 1.03f);
                 
                _triangles.Add(vertexIndex);
                _triangles.Add(vertexIndex + 2);
                _triangles.Add(vertexIndex + 1);
                _triangles.Add(vertexIndex);
                _triangles.Add(vertexIndex + 3);
                _triangles.Add(vertexIndex + 2);
                _normal = new Vector3(0, 1, 0);
            }
            else if (direction == 2) // down
            {
                _faceVertices[0] = _blockPosition + new Vector3(0, 0, 1);
                _faceVertices[1] = _blockPosition + new Vector3(1, 0, 1);
                _faceVertices[2] = _blockPosition + new Vector3(1, 0, 0);
                _faceVertices[3] = _blockPosition + new Vector3(0, 0, 0);
                _triangles.Add(vertexIndex);
                _triangles.Add(vertexIndex + 2);
                _triangles.Add(vertexIndex + 1);
                _triangles.Add(vertexIndex);
                _triangles.Add(vertexIndex + 3);
                _triangles.Add(vertexIndex + 2);
                _normal = new Vector3(0, -1, 0);
            }
            else if (direction == 3) // back
            {
                _faceVertices[0] = _blockPosition + new Vector3(0, 0, 1);
                _faceVertices[1] = _blockPosition + new Vector3(1, 0, 1);
                _faceVertices[2] = _blockPosition + new Vector3(1, 1, 1);
                _faceVertices[3] = _blockPosition + new Vector3(0, 1, 1);
                _triangles.Add(vertexIndex);
                _triangles.Add(vertexIndex + 1);
                _triangles.Add(vertexIndex + 2);
                _triangles.Add(vertexIndex);
                _triangles.Add(vertexIndex + 2);
                _triangles.Add(vertexIndex + 3);
                _normal = new Vector3(0, 0, 1);
            }
            else if (direction == 4) // front
            {
                _faceVertices[0] = _blockPosition + new Vector3(0, 0, 0);
                _faceVertices[1] = _blockPosition + new Vector3(1, 0, 0);
                _faceVertices[2] = _blockPosition + new Vector3(1, 1, 0);
                _faceVertices[3] = _blockPosition + new Vector3(0, 1, 0);
                _triangles.Add(vertexIndex);
                _triangles.Add(vertexIndex + 2);
                _triangles.Add(vertexIndex + 1);
                _triangles.Add(vertexIndex);
                _triangles.Add(vertexIndex + 3);
                _triangles.Add(vertexIndex + 2);
                _normal = new Vector3(0, 0, -1);
            }
            else if (direction == 5) // left
            {
                _faceVertices[0] = _blockPosition + new Vector3(0, 0, 0);
                _faceVertices[1] = _blockPosition + new Vector3(0, 0, 1);
                _faceVertices[2] = _blockPosition + new Vector3(0, 1, 1);
                _faceVertices[3] = _blockPosition + new Vector3(0, 1, 0);
                _triangles.Add(vertexIndex);
                _triangles.Add(vertexIndex + 1);
                _triangles.Add(vertexIndex + 2);
                _triangles.Add(vertexIndex);
                _triangles.Add(vertexIndex + 2);
                _triangles.Add(vertexIndex + 3);
                _normal = new Vector3(-1, 0, 0);
            }
            else if (direction == 6) // right
            {
                _faceVertices[0] = _blockPosition + new Vector3(1, 0, 0);
                _faceVertices[1] = _blockPosition + new Vector3(1, 0, 1);
                _faceVertices[2] = _blockPosition + new Vector3(1, 1, 1);
                _faceVertices[3] = _blockPosition + new Vector3(1, 1, 0);
                _triangles.Add(vertexIndex);
                _triangles.Add(vertexIndex + 2);
                _triangles.Add(vertexIndex + 1);
                _triangles.Add(vertexIndex);
                _triangles.Add(vertexIndex + 3);
                _triangles.Add(vertexIndex + 2);
                _normal = new Vector3(1, 0, 0);
            }
 
 
            Vector2Int tile = GetTileRect(textureIndex);
            Vector2[] _spriteUVs = new Vector2[]
            {
                new Vector2(tile.x, tile.y),
                new Vector2(tile.x + _tileSize, tile.y),
                new Vector2(tile.x + _tileSize, tile.y + _tileSize),
                new Vector2(tile.x, tile.y + _tileSize)
            };

            _textureRect = _textureRectDictionary[_blockID];

            // Calculate the new UVs based on the original rect's position and size
            for (int i = 0; i < _spriteUVs.Length; i++)
            {
                _spriteUVs[i] = new Vector2(
                    (_spriteUVs[i].x / _textureAtlasWidth) + _textureRect.x,
                    (_spriteUVs[i].y / _textureAtlasHeight) + _textureRect.y
                );
            }
 
            // Add each UV coordinate individually
            for (int i = 0; i < _spriteUVs.Length; i++)
            {
                _uvs.Add(_spriteUVs[i]);
                _normals.Add(_normal);
                _vertices.Add(_faceVertices[i]);

                if (direction == 1) 
                {
                    _verticesShadow.Add(_faceVerticesShadow[i]);
                }
                else
                {
                    _verticesShadow.Add(_faceVertices[i]);
                }
            } 
        }

        Vector2Int GetTileRect(int index)
        { 
            int targetRow = index / _tilesPerRow;
            int targetCol = index % _tilesPerRow;  

            return new Vector2Int(_colx[targetCol], _rowy[targetRow]);
        }
        void HandleBlockProcessData(int x, int y, int z)
        {

            bool blockAbove = false;
            if (y != _chunkDepth - 1) {
                blockAbove = _chunkMap[x, y + 1, z] != 0;
            } 

            bool blockFront = false;
            if (z != 0) {
                blockFront = _chunkMap[x, y, z - 1] != 0;
            }
            else
            {
                if (_frontFace.IsValid)
                {
                    blockFront = _frontFace[y, x];
                }
            }

            bool blockBack = false;
            if (z != _chunkSize - 1) {
                blockBack = _chunkMap[x, y, z + 1] != 0;
            }
            else
            {
                if (_backFace.IsValid)
                {
                    blockBack = _backFace[y, x];
                } 
            }

            bool blockLeft = false;
            if (x != 0) {
                blockLeft = _chunkMap[x - 1, y, z] != 0;
            }
            else
            {
                if (_leftFace.IsValid)
                {
                    blockLeft = _leftFace[y, z];
                }  
            }

            bool blockRight = false;
            if (x != _chunkSize - 1) {
                blockRight = _chunkMap[x + 1, y, z] != 0;
            }
            else
            {
                if (_rightFace.IsValid)
                {
                    blockRight = _rightFace[y, z];
                }  
            }

            if (!blockFront && !blockBack)
            {
                _blockData[5] = 1;
            }
            else if (!blockFront)
            {
                _blockData[5] = 2;
            }
            else if (!blockBack)
            {
                _blockData[5] = 3;
            }

            if (!blockLeft && !blockRight)
            {
                _blockData[3] = 1;
            }
            else if (!blockLeft)
            {
                _blockData[3] = 2;
            }
            else if (!blockRight)
            {
                _blockData[3] = 3;
            }

            // get autotile
            if (!blockAbove) {
                _blockData[0] = HandleMeshAutoTile(x, y, z, 1);
            } else {
                _blockData[0] = -1;
            }

            if (!blockFront || !blockBack) {
                _blockData[1] = HandleMeshAutoTile(x, y, z, 2);
            } else {
                _blockData[1] = -1;
            }

            if (!blockLeft || !blockRight) {
                _blockData[2] = HandleMeshAutoTile(x, y, z, 3);
            } else {
                _blockData[2] = -1;
            }

            if (y == 0 || _chunkMap[x, y - 1, z] == 0) { // bottom
                _blockData[4] = 1;
            } else {
                _blockData[4] = -1;
            }

            return;
        }

        int HandleMeshAutoTile(int x, int y, int z, int mode)
        {
            //! GM efficient Auto-tile sprite script
            // Original script by Taylor Lopez, edited
            // See the original script on Git Hub: https://github.com/iAmMortos/autotile

            //mode 1 top
            //mode 2 front
            //mode 3 sides
            spriteNumber = 0;
            edgeValue = 0;
            cornerValue = 0; 

            if (mode == 1){
                // Calculate edge values
                //check out of bounds || check layer || check layer on top
                isBackEdge = !IsValid(x, y, z + 1);
                isRightEdge = !IsValid(x + 1, y, z);
                isFrontEdge = !IsValid(x, y, z - 1);
                isLeftEdge = !IsValid(x - 1, y, z);

                isTop = !IsValid(x, y + 1, z);
                // bool isBackEdgeYPlus1 = !IsValid(x, y + 1, z + 1);
                // bool isRightEdgeYPlus1 = !IsValid(x + 1, y + 1, z);
                // bool isFrontEdgeYPlus1 = !IsValid(x, y + 1, z - 1);
                // bool isLeftEdgeYPlus1 = !IsValid(x - 1, y + 1, z);

                if ((isBackEdge && _backFace[y,x])
                || (!isBackEdge && (_chunkMap[x, y, z + 1] != 0))
                || (!isTop && isBackEdge && _backFace[y+1,x]) 
                || (!isTop && !isBackEdge && (_chunkMap[x, y+1, z + 1] != 0)
                )) edgeValue += 1; // Top

                if ((isRightEdge && _rightFace[y,z] )
                || (!isRightEdge && (_chunkMap[x + 1, y, z] != 0))
                || (!isTop && isRightEdge && _rightFace[y+1,z]) 
                || (!isTop && !isRightEdge && (_chunkMap[x + 1, y+1, z] != 0))
                ) edgeValue += 2; // Right

                if ((isFrontEdge && _frontFace[y,x] )
                || (!isFrontEdge && (_chunkMap[x, y, z - 1] != 0 ))
                || (!isTop && isFrontEdge && _frontFace[y+1,x]) 
                || (!isTop && !isFrontEdge && (_chunkMap[x, y+1, z - 1] != 0))
                ) edgeValue += 4; // Bottom

                if ((isLeftEdge && _leftFace[y,z]) 
                || (!isLeftEdge && (_chunkMap[x - 1, y, z] != 0))
                || (!isTop && isLeftEdge && _leftFace[y+1,z]) 
                || (!isTop && !isLeftEdge && (_chunkMap[x - 1, y+1, z] != 0))
                ) edgeValue += 8; // Left

                
                // Calculate _corner values
                if (((isBackEdge && !isLeftEdge && _backFace[y,x-1]) 
                    || (!isBackEdge && isLeftEdge && _leftFace[y,z+1]) 
                    || (isBackEdge && isLeftEdge && _corner[0,y]) 
                    || IsValid(x - 1, y, z + 1) && _chunkMap[x - 1, y, z + 1] != 0 
                    || (!isTop && isBackEdge && !isLeftEdge && _backFace[y+1,x-1]) 
                    || (!isTop && !isBackEdge && isLeftEdge && _leftFace[y+1,z+1])
                    || (!isTop && isBackEdge && isLeftEdge && _corner[0,y+1])
                    || (IsValid(x - 1, y + 1, z + 1) && _chunkMap[x - 1, y + 1, z + 1] != 0)) 
                    && (edgeValue & 1) != 0 && (edgeValue & 8) != 0) cornerValue += 1; // Top-Left

                if (((isBackEdge && !isRightEdge && _backFace[y,x+1]) 
                    || (!isBackEdge && isRightEdge && _rightFace[y,z+1]) 
                    || (isBackEdge && isRightEdge && _corner[1,y]) 
                    || IsValid(x + 1, y, z + 1) && _chunkMap[x + 1, y, z + 1] != 0 
                    || (!isTop && isBackEdge && !isRightEdge && _backFace[y+1,x+1]) 
                    || (!isTop && !isBackEdge && isRightEdge && _rightFace[y+1,z+1])
                    || (!isTop && isBackEdge && isRightEdge && _corner[1,y+1])
                    || (IsValid(x + 1, y + 1, z + 1) && _chunkMap[x + 1, y + 1, z + 1] != 0)) 
                    && (edgeValue & 1) != 0 && (edgeValue & 2) != 0) cornerValue += 2; // Top-Right

                if (((isFrontEdge && !isRightEdge && _frontFace[y,x+1]) 
                    || (!isFrontEdge && isRightEdge && _rightFace[y,z-1]) 
                    || (isFrontEdge && isRightEdge && _corner[3,y]) 
                    || (IsValid(x + 1, y, z - 1) && _chunkMap[x + 1, y, z - 1] != 0)
                    || (!isTop && isFrontEdge && !isRightEdge && _frontFace[y+1,x+1]) 
                    || (!isTop && !isFrontEdge && isRightEdge && _rightFace[y+1,z-1])
                    || (!isTop && isFrontEdge && isRightEdge && _corner[3,y+1])
                    || (IsValid(x + 1, y + 1, z - 1) && _chunkMap[x + 1, y + 1, z - 1] != 0)) 
                    && (edgeValue & 2) != 0 && (edgeValue & 4) != 0) cornerValue += 4; // Bottom-Right
                if (((isFrontEdge && !isLeftEdge && _frontFace[y,x-1]) 
                    || (!isFrontEdge && isLeftEdge && _leftFace[y,z-1]) 
                    || (isFrontEdge && isLeftEdge && _corner[2,y]) 
                    || IsValid(x - 1, y, z - 1) && _chunkMap[x - 1, y, z - 1] != 0 
                    || (!isTop && isFrontEdge && !isLeftEdge && _frontFace[y+1,x-1]) 
                    || (!isTop && !isFrontEdge && isLeftEdge && _leftFace[y+1,z-1])
                    || (!isTop && isFrontEdge && isLeftEdge && _corner[2,y+1])
                    || (IsValid(x - 1, y + 1, z - 1) && _chunkMap[x - 1, y + 1, z - 1] != 0)) 
                    && (edgeValue & 4) != 0 && (edgeValue & 8) != 0) cornerValue += 8; // Bottom-Left


            } 
            else if (mode == 2)
            { 
                
                isRightEdge = !IsValid(x + 1, y, z);
                isLeftEdge = !IsValid(x - 1, y, z);
                isTop = !IsValid(x, y + 1, z);
                isBottem = !IsValid(x, y - 1, z);

                // Calculate edge values
                if (!isTop && _chunkMap[x, y + 1, z] != 0) edgeValue += 1; // Top
                if (!isRightEdge && _chunkMap[x + 1, y, z] != 0
                || isRightEdge && _rightFace[y,z]) edgeValue += 2; // Right
                if (!isBottem && _chunkMap[x, y - 1, z] != 0) edgeValue += 4; // Bottom
                if (!isLeftEdge && _chunkMap[x - 1, y, z] != 0
                || isLeftEdge && _leftFace[y,z]) edgeValue += 8; // Left

                // Calculate _corner values
                if ((!isTop && !isLeftEdge && _chunkMap[x - 1, y + 1, z] != 0 
                || !isTop && isLeftEdge && _leftFace[y+1,z])
                && (edgeValue & 1) != 0 && (edgeValue & 8) != 0) cornerValue += 1; // Top-Left

                if ((!isTop && !isRightEdge && _chunkMap[x + 1, y + 1, z] != 0
                || !isTop && isRightEdge && _rightFace[y+1,z])
                && (edgeValue & 1) != 0 && (edgeValue & 2) != 0) cornerValue += 2; // Top-Right

                if ((!isBottem && !isRightEdge && _chunkMap[x + 1, y - 1, z] != 0 
                || !isBottem && isRightEdge && _rightFace[y-1,z])
                && (edgeValue & 2) != 0 && (edgeValue & 4) != 0) cornerValue += 4; // Bottom-Right

                if ((!isBottem && !isLeftEdge && _chunkMap[x - 1, y - 1, z] != 0 
                || !isBottem && isLeftEdge && _leftFace[y-1,z])
                && (edgeValue & 4) != 0 && (edgeValue & 8) != 0) cornerValue += 8; // Bottom-Left
            }
            else 
            { 
                isFrontEdge = !IsValid(x, y, z - 1);
                isBackEdge = !IsValid(x, y, z + 1);
                isTop = !IsValid(x, y + 1, z);
                isBottem = !IsValid(x, y - 1, z);

                // Calculate edge values
                if (!isTop && _chunkMap[x, y + 1, z] != 0) edgeValue += 1; // Top

                if (!isBackEdge && _chunkMap[x, y, z + 1] != 0
                || isBackEdge && _backFace[y,x]) edgeValue += 2; // Right

                if (!isBottem && _chunkMap[x, y - 1, z] != 0) edgeValue += 4; // Bottom 
                
                if (!isFrontEdge && _chunkMap[x, y, z - 1] != 0
                || isFrontEdge && _frontFace[y,x]) edgeValue += 8; // Left

                // Calculate _corner values
                if ((!isTop && !isFrontEdge && _chunkMap[x, y + 1, z - 1] != 0 
                || !isTop && isFrontEdge && _frontFace[y+1,x])
                && (edgeValue & 1) != 0 && (edgeValue & 8) != 0) cornerValue += 1; // Top-Left
                if ((!isTop && !isBackEdge && _chunkMap[x, y + 1, z + 1] != 0 
                || !isTop && isBackEdge && _backFace[y+1,x])
                && (edgeValue & 1) != 0 && (edgeValue & 2) != 0) cornerValue += 2; // Top-Right
                if ((!isBottem && !isBackEdge && _chunkMap[x, y - 1, z + 1] != 0 
                || !isBottem && isBackEdge && _backFace[y-1,x])
                && (edgeValue & 2) != 0 && (edgeValue & 4) != 0) cornerValue += 4; // Bottom-Right
                if ((!isBottem && !isFrontEdge && _chunkMap[x, y - 1, z - 1] != 0 
                || !isBottem && isFrontEdge && _frontFace[y-1,x])
                && (edgeValue & 4) != 0 && (edgeValue & 8) != 0) cornerValue += 8; // Bottom-Left
            }

            // Determine the tile number using nested switch statements
            switch (edgeValue)
            {
                case 0: spriteNumber = 36; break;
                case 1: spriteNumber = 24; break;
                case 2: spriteNumber = 37; break;
                case 3:
                    switch (cornerValue)
                    {
                        case 0: spriteNumber = 25; break;
                        case 2: spriteNumber = 44; break;
                    }
                    break;
                case 4: spriteNumber = 0; break;
                case 5: spriteNumber = 12; break;
                case 6:
                    switch (cornerValue)
                    {
                        case 0: spriteNumber = 1; break;
                        case 4: spriteNumber = 8; break;
                    }
                    break;
                case 7:
                    switch (cornerValue)
                    {
                        case 0: spriteNumber = 13; break;
                        case 2: spriteNumber = 28; break;
                        case 4: spriteNumber = 16; break;
                        case 6: spriteNumber = 20; break;
                    }
                    break;
                case 8: spriteNumber = 39; break;
                case 9:
                    switch (cornerValue)
                    {
                        case 0: spriteNumber = 27; break;
                        case 1: spriteNumber = 47; break;
                    }
                    break;
                case 10: spriteNumber = 38; break;
                case 11:
                    switch (cornerValue)
                    {
                        case 0: spriteNumber = 26; break;
                        case 1: spriteNumber = 42; break;
                        case 2: spriteNumber = 41; break;
                        case 3: spriteNumber = 45; break;
                    }
                    break;
                case 12:
                    switch (cornerValue)
                    {
                        case 0: spriteNumber = 3; break;
                        case 8: spriteNumber = 11; break;
                    }
                    break;
                case 13:
                    switch (cornerValue)
                    {
                        case 0: spriteNumber = 15; break;
                        case 1: spriteNumber = 31; break;
                        case 8: spriteNumber = 19; break;
                        case 9: spriteNumber = 35; break;
                    }
                    break;
                case 14:
                    switch (cornerValue)
                    {
                        case 0: spriteNumber = 2; break;
                        case 4: spriteNumber = 5; break;
                        case 8: spriteNumber = 6; break;
                        case 12: spriteNumber = 10; break;
                    }
                    break;
                case 15:
                    switch (cornerValue)
                    {
                        case 0: spriteNumber = 14; break;
                        case 1: spriteNumber = 4; break;
                        case 2: spriteNumber = 7; break;
                        case 3: spriteNumber = 46; break;
                        case 4: spriteNumber = 43; break;
                        case 5: spriteNumber = 34; break;
                        case 6: spriteNumber = 32; break;
                        case 7: spriteNumber = 29; break;
                        case 8: spriteNumber = 40; break;
                        case 9: spriteNumber = 23; break;
                        case 10: spriteNumber = 21; break;
                        case 11: spriteNumber = 30; break;
                        case 12: spriteNumber = 9; break;
                        case 13: spriteNumber = 18; break;
                        case 14: spriteNumber = 17; break;
                        case 15: spriteNumber = 33; break;
                    }
                    break;
            }
            
            if (mode != 1){
                spriteNumber += 48;
            }
            return spriteNumber;
        }

        bool IsValid(int x, int y, int z)
        {
            return x >= 0 && x < _chunkSize && y >= 0 && y < _chunkDepth && z >= 0 && z < _chunkSize;
        }
    }

    public (int[,,], bool[,], bool[,], bool[,], bool[,], bool[,]) LoadChunkMap(Vector3Int coordinates, WorldStatic worldStatic)
    {
        ChunkData _targetChunkDataTemp;
        int[,,] _targetChunk;
        bool[,] _frontFace, _backFace, _leftFace, _rightFace, _corner;
        
        _frontFace = null;
        _backFace = null;
        _leftFace = null;
        _rightFace = null;
        _corner = new bool[4, WorldStatic.CHUNKDEPTH];

        _targetChunkDataTemp = worldStatic.GetChunk(new Vector3Int(coordinates.x, coordinates.y, coordinates.z - WorldStatic.CHUNKSIZE));
        if (_targetChunkDataTemp != null)
        {
            _frontFace = new bool[WorldStatic.CHUNKDEPTH, WorldStatic.CHUNKSIZE];
            for (int y = 0; y < WorldStatic.CHUNKDEPTH; y++)
            {
                for (int x = 0; x < WorldStatic.CHUNKSIZE; x++)
                {
                    _frontFace[y, x] = !IsAir(_targetChunkDataTemp.Map[x, y, WorldStatic.CHUNKSIZE - 1]);
                }
            }
        } 

        _targetChunkDataTemp = worldStatic.GetChunk(new Vector3Int(coordinates.x, coordinates.y, coordinates.z + WorldStatic.CHUNKSIZE));
        if (_targetChunkDataTemp != null)
        {
            _backFace = new bool[WorldStatic.CHUNKDEPTH, WorldStatic.CHUNKSIZE];
            for (int y = 0; y < WorldStatic.CHUNKDEPTH; y++)
            {
                for (int x = 0; x < WorldStatic.CHUNKSIZE; x++)
                {
                    _backFace[y, x] = !IsAir(_targetChunkDataTemp.Map[x, y, 0]);
                }
            }
        } 

        _targetChunkDataTemp = worldStatic.GetChunk(new Vector3Int(coordinates.x - WorldStatic.CHUNKSIZE, coordinates.y, coordinates.z));
        if (_targetChunkDataTemp != null)
        {
            _leftFace = new bool[WorldStatic.CHUNKDEPTH, WorldStatic.CHUNKSIZE];
            for (int y = 0; y < WorldStatic.CHUNKDEPTH; y++)
            {
                for (int z = 0; z < WorldStatic.CHUNKSIZE; z++)
                {
                    _leftFace[y, z] = !IsAir(_targetChunkDataTemp.Map[WorldStatic.CHUNKSIZE - 1, y, z]);
                }
            }   
        }  

        _targetChunkDataTemp = worldStatic.GetChunk(new Vector3Int(coordinates.x + WorldStatic.CHUNKSIZE, coordinates.y, coordinates.z));
        if (_targetChunkDataTemp != null)
        {
            _rightFace = new bool[WorldStatic.CHUNKDEPTH, WorldStatic.CHUNKSIZE];
            for (int y = 0; y < WorldStatic.CHUNKDEPTH; y++)
            {
                for (int z = 0; z < WorldStatic.CHUNKSIZE; z++)
                {
                    _rightFace[y, z] = !IsAir(_targetChunkDataTemp.Map[0, y, z]);
                }
            }
        } 
        
        _targetChunkDataTemp = worldStatic.GetChunk(new Vector3Int(coordinates.x - WorldStatic.CHUNKSIZE, coordinates.y, coordinates.z + WorldStatic.CHUNKSIZE));
        if (_targetChunkDataTemp != null)
        {
            for (int y = 0; y < WorldStatic.CHUNKDEPTH; y++)
            {
                _corner[0, y] = !IsAir(_targetChunkDataTemp.Map[WorldStatic.CHUNKSIZE - 1, y, 0]);
            }
        }

        _targetChunkDataTemp = worldStatic.GetChunk(new Vector3Int(coordinates.x + WorldStatic.CHUNKSIZE, coordinates.y, coordinates.z + WorldStatic.CHUNKSIZE));
        if (_targetChunkDataTemp != null)
        {
            for (int y = 0; y < WorldStatic.CHUNKDEPTH; y++)
            {
                _corner[1, y] = !IsAir(_targetChunkDataTemp.Map[0, y, 0]);
            }
        }

        _targetChunkDataTemp = worldStatic.GetChunk(new Vector3Int(coordinates.x - WorldStatic.CHUNKSIZE, coordinates.y, coordinates.z - WorldStatic.CHUNKSIZE));
        if (_targetChunkDataTemp != null)
        {
            for (int y = 0; y < WorldStatic.CHUNKDEPTH; y++)
            {
                _corner[2, y] = !IsAir(_targetChunkDataTemp.Map[WorldStatic.CHUNKSIZE - 1, y, WorldStatic.CHUNKSIZE - 1]);
            }
        }

        _targetChunkDataTemp = worldStatic.GetChunk(new Vector3Int(coordinates.x + WorldStatic.CHUNKSIZE, coordinates.y, coordinates.z - WorldStatic.CHUNKSIZE));
        if (_targetChunkDataTemp != null)
        {
            for (int y = 0; y < WorldStatic.CHUNKDEPTH; y++)
            {
                _corner[3, y] = !IsAir(_targetChunkDataTemp.Map[0, y, WorldStatic.CHUNKSIZE - 1]);
            } 
        }

        ChunkData chunkDataTemp = worldStatic.GetChunk(coordinates);
        _targetChunk = chunkDataTemp == null? null : chunkDataTemp.Map;
        return (_targetChunk, _frontFace, _backFace, _leftFace, _rightFace, _corner);
    }

    public bool IsAir(int ID)
    {
        return ID == 0;
    }

    public int GetBlockInChunk(Vector3Int chunkCoordinate, Vector3Int blockCoordinate, WorldStatic worldStatic) //0 = empty, 1 = block, error = out of bounds
    {
        try
        {
            if (blockCoordinate.y >= 0 && blockCoordinate.y < WorldStatic.CHUNKDEPTH)
            {
                var chunk = worldStatic.GetChunk(chunkCoordinate);
                if (chunk != null)
                {
                    return chunk.Map[blockCoordinate.x, blockCoordinate.y, blockCoordinate.z];
                }
            }
            return 0;
        }
        catch
        {
            Lib.Log("error in isblocknull");
            return 0;
        }
    }
}










    public struct NativeArray2D<T> where T : struct
    {
        private NativeArray<T> array;
        private int rows;
        private int cols;
        public bool IsValid;

        public NativeArray2D(T[,] array, Allocator allocator)
        {
            rows = array.GetLength(0);
            cols = array.GetLength(1);
            this.array = new NativeArray<T>(rows * cols, allocator);
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    this.array[i * cols + j] = array[i, j];
                }
            }
            IsValid = true;
        }

        public T this[int row, int col]
        {
            get => array[row * cols + col];
            set => array[row * cols + col] = value;
        }

        public void Dispose()
        {
            array.Dispose();
        }
    }

    public struct NativeArray3D<T> where T : struct
    {
        private NativeArray<T> array;
        private int rows;
        private int cols;
        private int depth;

        public NativeArray3D(T[,,] array, Allocator allocator)
        {
            rows = array.GetLength(0);
            cols = array.GetLength(1);
            depth = array.GetLength(2);
            this.array = new NativeArray<T>(rows * cols * depth, allocator);
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    for (int k = 0; k < depth; k++)
                    {
                        this.array[i * cols * depth + j * depth + k] = array[i, j, k];
                    }
                }
            }
        }

        public T this[int row, int col, int dep]
        {
            get => array[row * cols * depth + col * depth + dep];
            set => array[row * cols * depth + col * depth + dep] = value;
        }

        public void Dispose()
        {
            array.Dispose();
        }
    }