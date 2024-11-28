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

        _tileSize = 16;
        _tilesPerRow = 12;
        _colx = new int[] {0, 16, 32, 48, 64, 80, 96, 112, 128, 144, 160, 176};
        _rowy = new int[] {112, 96, 80, 64, 48, 32, 16, 0};  
    } 

    async void Start()
    {
        await Task.Delay(50);
        HandleChunkMapTraverse();
        EntityLoadStatic.Instance.OnTraverse();
        WorldStatic.Instance.GenerateBoolMap();
    }















    
    public void RefreshExistingChunk(Vector3Int chunkCoordinates)
    {
        if (!WorldStatic.Instance.IsInWorldBounds(chunkCoordinates)) return;
        _ = LoadChunksOntoScreenAsync(chunkCoordinates, true);
    }
 

    Vector3 traverseCheckPosition;
    List<Vector3Int> destroyList = new List<Vector3Int>();
    void HandleChunkMapTraverse()
    { 
        foreach (var kvp in _activeChunks)
        {
            traverseCheckPosition = kvp.Key;
            if (kvp.Key.x > WorldStatic._playerChunkPos.x + WorldStatic.RENDER_DISTANCE * WorldStatic.CHUNKSIZE || kvp.Key.x < WorldStatic._playerChunkPos.x - WorldStatic.RENDER_DISTANCE * WorldStatic.CHUNKSIZE
                || kvp.Key.y > WorldStatic._playerChunkPos.y + WorldStatic.RENDER_DISTANCE * WorldStatic.CHUNKSIZE || kvp.Key.y < WorldStatic._playerChunkPos.y - WorldStatic.RENDER_DISTANCE * WorldStatic.CHUNKSIZE
                || kvp.Key.z > WorldStatic._playerChunkPos.z + WorldStatic.RENDER_DISTANCE * WorldStatic.CHUNKSIZE || kvp.Key.z < WorldStatic._playerChunkPos.z - WorldStatic.RENDER_DISTANCE * WorldStatic.CHUNKSIZE)
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
            for (int y = -WorldStatic.RENDER_DISTANCE; y <= WorldStatic.RENDER_DISTANCE; y++)
            {
                for (int z = -WorldStatic.RENDER_DISTANCE; z <= WorldStatic.RENDER_DISTANCE; z++)
                {
                    Vector3Int traverseCheckPosition = new Vector3Int(
                        WorldStatic._playerChunkPos.x + x * WorldStatic.CHUNKSIZE,
                        WorldStatic._playerChunkPos.y + y * WorldStatic.CHUNKSIZE,
                        WorldStatic._playerChunkPos.z + z * WorldStatic.CHUNKSIZE
                    );
                    if (!_activeChunks.ContainsKey(traverseCheckPosition) && WorldStatic.Instance.IsInWorldBounds(traverseCheckPosition))
                        _ = LoadChunksOntoScreenAsync(traverseCheckPosition);
                }
            }
        } 
    }

    public Dictionary<Vector3Int, GameObject> _activeChunks = new Dictionary<Vector3Int, GameObject>();
    private SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1); 
    private async Task LoadChunksOntoScreenAsync(Vector3Int chunkCoord, bool replace = false)
    { 
        await _semaphoreSlim.WaitAsync();
        try
        { 
            if (replace || !_activeChunks.ContainsKey(chunkCoord))
            {
                _chunkCoordinate = chunkCoord;
                _chunkData = WorldStatic.World[chunkCoord.x, chunkCoord.y, chunkCoord.z];
                if (_chunkData != ChunkData.Zero)
                {
                    await Task.Run(() => LoadMeshMath()); 
                    await Task.Delay(10);
                    LoadMeshObject(replace);
                }  else Lib.Log("Chunk in queue is zero");
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

    private Mesh _mesh; 
    private GameObject _meshObject;
    private MeshRenderer _meshRenderer;
    private MapCullInst _mapCullInst;
    private int counter;
    private void LoadMeshObject(bool replace = false)
    {
        _mesh = new Mesh();
        _mesh.SetVertices(_vertices);
        _mesh.SetTriangles(_triangles, 0);
        _mesh.SetUVs(0, _uvs);
        _mesh.SetNormals(_normals); 
 
        if (!replace)
        {
            _meshObject = new("MAP " + counter++);
            _meshObject.layer = LayerMask.NameToLayer("Collision");
            _meshObject.transform.position = _chunkCoordinate; 

            _meshObject.AddComponent<MeshFilter>();
            _meshRenderer = _meshObject.AddComponent<MeshRenderer>();  
            _meshRenderer.material = BlockStatic._meshMaterial; 

            _mapCullInst = _meshObject.AddComponent<MapCullInst>();  
            _mapCullInst._chunkMap = _chunkData.Map;
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
            _mapCullInst._chunkMap = _chunkData.Map;
            _mapCullInst._meshData = _mesh;
            _mapCullInst._verticesShadow = _verticesShadow;
            _mapCullInst.HandleAssignment(); 
        } 
    }
 



    // const
    private int _tileSize;
    private int _tilesPerRow;
    private int[] _colx;
    private int[] _rowy; 

    // input
    private Vector3Int _chunkCoordinate;
    private ChunkData _chunkData;  

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
                _chunkSize = WorldStatic.CHUNKSIZE,
                _tileSize = _tileSize,
                _tilesPerRow = _tilesPerRow, 
                _colx = new NativeArray<int>(_colx, Allocator.TempJob),
                _rowy = new NativeArray<int>(_rowy, Allocator.TempJob),  
                _textureRectDictionary = _textureRectDictionary,
                _textureAtlasWidth = BlockStatic._textureAtlasWidth,
                _textureAtlasHeight = BlockStatic._textureAtlasHeight, 

                // input 
                _chunkMap = ChunkMap.Create(_chunkCoordinate),

                // output
                _vertices = new NativeList<Vector3>(Allocator.TempJob),
                _verticesShadow = new NativeList<Vector3>(Allocator.TempJob),
                _triangles = new NativeList<int>(Allocator.TempJob),
                _uvs = new NativeList<Vector2>(Allocator.TempJob),
                _normals = new NativeList<Vector3>(Allocator.TempJob),

                // local temp
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
         

    // [BurstCompile(CompileSynchronously = true)]
    public struct MeshMathJob : IJob
    {
        // const
        [DeallocateOnJobCompletion]
        public int _chunkSize; 
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
        public NativeMap3D<int> _chunkMap;  

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
        public int py;
        [DeallocateOnJobCompletion]
        public int ny;
        [DeallocateOnJobCompletion]
        public int px;
        [DeallocateOnJobCompletion]
        public int nx;
        [DeallocateOnJobCompletion]
        public int pz;
        [DeallocateOnJobCompletion]
        public int nz;

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
        private Rect _textureRect;
 
        public void Execute()
        {   
            for (int z = 0; z < _chunkSize; z++)
            {
                for (int x = 0; x < _chunkSize; x++)
                {
                    for (int y = 0; y < _chunkSize; y++)
                    {
                        _blockID = _chunkMap[x, y, z];

                        if (_blockID != 0)
                        {
                            _blockPosition = new Vector3(x, y, z);

                            if (_chunkMap[x, y + 1, z] == 0) HandleMeshFace(dir.py, HandleMeshAutoTile(x, y, z, dir.py)); // Top
                            if (_chunkMap[x, y - 1, z] == 0) HandleMeshFace(dir.ny, HandleMeshAutoTile(x, y, z, dir.ny)); // Bottom
                            if (_chunkMap[x + 1, y, z] == 0) HandleMeshFace(dir.px, HandleMeshAutoTile(x, y, z, dir.px)); // Right
                            if (_chunkMap[x - 1, y, z] == 0) HandleMeshFace(dir.nx, HandleMeshAutoTile(x, y, z, dir.nx)); // Left
                            if (_chunkMap[x, y, z + 1] == 0) HandleMeshFace(dir.pz, HandleMeshAutoTile(x, y, z, dir.pz)); // Front
                            if (_chunkMap[x, y, z - 1] == 0) HandleMeshFace(dir.nz, HandleMeshAutoTile(x, y, z, dir.nz)); // Back
                        }
                    }
                }
            }
        }

        void HandleMeshFace(dir direction, int textureIndex)
        { 
            int vertexIndex = _vertices.Length; 
            _normal = Vector3.zero;

            if (direction == dir.py) // top
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
            else if (direction == dir.ny) // down
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
            else if (direction == dir.pz) // back
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
            else if (direction == dir.nz) // front
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
            else if (direction == dir.nx) // left
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
            else if (direction == dir.px) // right
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

                if (direction == dir.py) 
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
         
        enum dir { px, nx, py, ny, pz, nz }

        int HandleMeshAutoTile(int x, int y, int z, dir mode)
        {
            spriteNumber = 0;
            edgeValue = 0;
            cornerValue = 0;

            if (mode == dir.py) // Top
            {
                bool isPy = _chunkMap[x, y + 1, z] == 0;

                if ((_chunkMap[x, y, z + 1] != 0)
                    || (isPy && _chunkMap[x, y + 1, z + 1] != 0))
                {
                    edgeValue += 1; // Top
                }

                if ((_chunkMap[x + 1, y, z] != 0)
                    || (isPy && _chunkMap[x + 1, y + 1, z] != 0))
                {
                    edgeValue += 2; // Right
                }

                if ((_chunkMap[x, y, z - 1] != 0)
                    || (isPy && _chunkMap[x, y + 1, z - 1] != 0))
                {
                    edgeValue += 4; // Bottom
                }

                if ((_chunkMap[x - 1, y, z] != 0)
                    || (isPy && _chunkMap[x - 1, y + 1, z] != 0))
                {
                    edgeValue += 8; // Left
                }

                // Calculate corner values
                if (((_chunkMap[x - 1, y, z + 1] != 0)
                    || (isPy && _chunkMap[x - 1, y + 1, z + 1] != 0))
                    && (edgeValue & 1) != 0 && (edgeValue & 8) != 0)
                {
                    cornerValue += 1; // Top-Left
                }

                if (((_chunkMap[x + 1, y, z + 1] != 0)
                    || (isPy && _chunkMap[x + 1, y + 1, z + 1] != 0))
                    && (edgeValue & 1) != 0 && (edgeValue & 2) != 0)
                {
                    cornerValue += 2; // Top-Right
                }

                if (((_chunkMap[x + 1, y, z - 1] != 0)
                    || (isPy && _chunkMap[x + 1, y + 1, z - 1] != 0))
                    && (edgeValue & 2) != 0 && (edgeValue & 4) != 0)
                {
                    cornerValue += 4; // Bottom-Right
                }

                if (((_chunkMap[x - 1, y, z - 1] != 0)
                    || (isPy && _chunkMap[x - 1, y + 1, z - 1] != 0))
                    && (edgeValue & 4) != 0 && (edgeValue & 8) != 0)
                {
                    cornerValue += 8; // Bottom-Left
                }
            } 
            else if (mode == dir.ny) // Negative Y (Bottom of the cube)
            {
                bool isNy = _chunkMap[x, y - 1, z] == 0;

                if ((_chunkMap[x, y, z + 1] != 0)
                    || (isNy && _chunkMap[x, y - 1, z + 1] != 0))
                {
                    edgeValue += 1; // Top
                }

                if ((_chunkMap[x + 1, y, z] != 0)
                    || (isNy && _chunkMap[x + 1, y - 1, z] != 0))
                {
                    edgeValue += 2; // Right
                }

                if ((_chunkMap[x, y, z - 1] != 0)
                    || (isNy && _chunkMap[x, y - 1, z - 1] != 0))
                {
                    edgeValue += 4; // Bottom
                }

                if ((_chunkMap[x - 1, y, z] != 0)
                    || (isNy && _chunkMap[x - 1, y - 1, z] != 0))
                {
                    edgeValue += 8; // Left
                }

                // Calculate corner values
                if (((_chunkMap[x - 1, y, z + 1] != 0)
                     || (isNy && _chunkMap[x - 1, y - 1, z + 1] != 0))
                    && (edgeValue & 1) != 0 && (edgeValue & 8) != 0)
                {
                    cornerValue += 1; // Top-Left
                }

                if (((_chunkMap[x + 1, y, z + 1] != 0)
                     || (isNy && _chunkMap[x + 1, y - 1, z + 1] != 0))
                    && (edgeValue & 1) != 0 && (edgeValue & 2) != 0)
                {
                    cornerValue += 2; // Top-Right
                }

                if (((_chunkMap[x + 1, y, z - 1] != 0)
                     || (isNy && _chunkMap[x + 1, y - 1, z - 1] != 0))
                    && (edgeValue & 2) != 0 && (edgeValue & 4) != 0)
                {
                    cornerValue += 4; // Bottom-Right
                }

                if (((_chunkMap[x - 1, y, z - 1] != 0)
                     || (isNy && _chunkMap[x - 1, y - 1, z - 1] != 0))
                    && (edgeValue & 4) != 0 && (edgeValue & 8) != 0)
                {
                    cornerValue += 8; // Bottom-Left
                }
            }
            else if (mode == dir.nx) // Negative X (Left side of the cube)
            {
                bool isNx = _chunkMap[x - 1, y, z] == 0;

                if ((_chunkMap[x, y + 1, z] != 0)
                    || (isNx && _chunkMap[x - 1, y + 1, z] != 0))
                {
                    edgeValue += 1; // Top
                }

                if ((_chunkMap[x, y, z + 1] != 0)
                    || (isNx && _chunkMap[x - 1, y, z + 1] != 0))
                {
                    edgeValue += 2; // Right
                }

                if ((_chunkMap[x, y - 1, z] != 0)
                    || (isNx && _chunkMap[x - 1, y - 1, z] != 0))
                {
                    edgeValue += 4; // Bottom
                }

                if ((_chunkMap[x, y, z - 1] != 0)
                    || (isNx && _chunkMap[x - 1, y, z - 1] != 0))
                {
                    edgeValue += 8; // Left
                }

                // Calculate corner values
                
                if (((_chunkMap[x, y + 1, z - 1] != 0)
                     || (isNx && _chunkMap[x - 1, y + 1, z - 1] != 0))
                    && (edgeValue & 1) != 0 && (edgeValue & 8) != 0)
                {
                    cornerValue += 1; // Top-Left
                }
                if (((_chunkMap[x, y + 1, z + 1] != 0)
                     || (isNx && _chunkMap[x - 1, y + 1, z + 1] != 0))
                    && (edgeValue & 1) != 0 && (edgeValue & 2) != 0)
                {
                    cornerValue += 2; // Top-Right
                }

                if (((_chunkMap[x, y - 1, z + 1] != 0)
                     || (isNx && _chunkMap[x - 1, y - 1, z + 1] != 0))
                    && (edgeValue & 2) != 0 && (edgeValue & 4) != 0)
                {
                    cornerValue += 4; // Bottom-Right
                }

                if (((_chunkMap[x, y - 1, z - 1] != 0)
                     || (isNx && _chunkMap[x - 1, y - 1, z - 1] != 0))
                    && (edgeValue & 4) != 0 && (edgeValue & 8) != 0)
                {
                    cornerValue += 8; // Bottom-Left
                } 
            }
            else if (mode == dir.px) // Positive X (Right side of the cube)
            {
                bool isPx = _chunkMap[x + 1, y, z] == 0;

                if ((_chunkMap[x, y + 1, z] != 0)
                    || (isPx && _chunkMap[x + 1, y + 1, z] != 0))
                {
                    edgeValue += 1; // Top
                }

                if ((_chunkMap[x, y, z + 1] != 0)
                    || (isPx && _chunkMap[x + 1, y, z + 1] != 0))
                {
                    edgeValue += 2; // Right
                }

                if ((_chunkMap[x, y - 1, z] != 0)
                    || (isPx && _chunkMap[x + 1, y - 1, z] != 0))
                {
                    edgeValue += 4; // Bottom
                }

                if ((_chunkMap[x, y, z - 1] != 0)
                    || (isPx && _chunkMap[x + 1, y, z - 1] != 0))
                {
                    edgeValue += 8; // Left
                }

                // Lib.Log(x,y,z,_chunkMap[x, y + 1, z] != 0, _chunkMap[x, y, z + 1] != 0,_chunkMap[x, y - 1, z] != 0, _chunkMap[x, y, z - 1] != 0, _chunkMap[x, y + 1, z + 1] != 0, _chunkMap[x, y - 1, z + 1] != 0, _chunkMap[x, y - 1, z - 1] != 0, _chunkMap[x, y + 1, z - 1] != 0);
                // Calculate corner values

                if (((_chunkMap[x, y + 1, z - 1] != 0)
                     || (isPx && _chunkMap[x + 1, y + 1, z - 1] != 0))
                    && (edgeValue & 1) != 0 && (edgeValue & 8) != 0)
                {
                    cornerValue += 1; // Top-Left
                }
                
                if (((_chunkMap[x, y + 1, z + 1] != 0)
                     || (isPx && _chunkMap[x + 1, y + 1, z + 1] != 0))
                    && (edgeValue & 1) != 0 && (edgeValue & 2) != 0)
                {
                    cornerValue += 2; // Top-Right
                }

                if (((_chunkMap[x, y - 1, z + 1] != 0)
                     || (isPx && _chunkMap[x + 1, y - 1, z + 1] != 0))
                    && (edgeValue & 2) != 0 && (edgeValue & 4) != 0)
                {
                    cornerValue += 4; // Bottom-Right
                }

                if (((_chunkMap[x, y - 1, z - 1] != 0)
                     || (isPx && _chunkMap[x + 1, y - 1, z - 1] != 0))
                    && (edgeValue & 4) != 0 && (edgeValue & 8) != 0)
                {
                    cornerValue += 8; // Bottom-Left
                } 
            }
            else if (mode == dir.pz) // Positive Z (Front of the cube)
            {
                bool isPz = _chunkMap[x, y, z + 1] == 0;

                if ((_chunkMap[x, y + 1, z] != 0)
                    || (isPz && _chunkMap[x, y + 1, z + 1] != 0))
                {
                    edgeValue += 1; // Top
                }

                if ((_chunkMap[x + 1, y, z] != 0)
                    || (isPz && _chunkMap[x + 1, y, z + 1] != 0))
                {
                    edgeValue += 2; // Right
                }

                if ((_chunkMap[x, y - 1, z] != 0)
                    || (isPz && _chunkMap[x, y - 1, z + 1] != 0))
                {
                    edgeValue += 4; // Bottom
                }

                if ((_chunkMap[x - 1, y, z] != 0)
                    || (isPz && _chunkMap[x - 1, y, z + 1] != 0))
                {
                    edgeValue += 8; // Left
                }

                // Calculate corner values
                if (((_chunkMap[x - 1, y + 1, z] != 0)
                     || (isPz && _chunkMap[x - 1, y + 1, z + 1] != 0))
                    && (edgeValue & 1) != 0 && (edgeValue & 8) != 0)
                {
                    cornerValue += 1; // Top-Left
                }

                if (((_chunkMap[x + 1, y + 1, z] != 0)
                     || (isPz && _chunkMap[x + 1, y + 1, z + 1] != 0))
                    && (edgeValue & 1) != 0 && (edgeValue & 2) != 0)
                {
                    cornerValue += 2; // Top-Right
                }

                if (((_chunkMap[x + 1, y - 1, z] != 0)
                     || (isPz && _chunkMap[x + 1, y - 1, z + 1] != 0))
                    && (edgeValue & 2) != 0 && (edgeValue & 4) != 0)
                {
                    cornerValue += 4; // Bottom-Right
                }

                if (((_chunkMap[x - 1, y - 1, z] != 0)
                     || (isPz && _chunkMap[x - 1, y - 1, z + 1] != 0))
                    && (edgeValue & 4) != 0 && (edgeValue & 8) != 0)
                {
                    cornerValue += 8; // Bottom-Left
                }
            }
            else if (mode == dir.nz) // Negative Z (Back of the cube)
            {
                bool isNz = _chunkMap[x, y, z - 1] == 0;

                if ((_chunkMap[x, y + 1, z] != 0)
                    || (isNz && _chunkMap[x, y + 1, z - 1] != 0))
                {
                    edgeValue += 1; // Top
                }

                if ((_chunkMap[x + 1, y, z] != 0)
                    || (isNz && _chunkMap[x + 1, y, z - 1] != 0))
                {
                    edgeValue += 2; // Right
                }

                if ((_chunkMap[x, y - 1, z] != 0)
                    || (isNz && _chunkMap[x, y - 1, z - 1] != 0))
                {
                    edgeValue += 4; // Bottom
                }

                if ((_chunkMap[x - 1, y, z] != 0)
                    || (isNz && _chunkMap[x - 1, y, z - 1] != 0))
                {
                    edgeValue += 8; // Left
                }

                // Calculate corner values
                if (((_chunkMap[x - 1, y + 1, z] != 0)
                     || (isNz && _chunkMap[x - 1, y + 1, z - 1] != 0))
                    && (edgeValue & 1) != 0 && (edgeValue & 8) != 0)
                {
                    cornerValue += 1; // Top-Left
                }

                if (((_chunkMap[x + 1, y + 1, z] != 0)
                     || (isNz && _chunkMap[x + 1, y + 1, z - 1] != 0))
                    && (edgeValue & 1) != 0 && (edgeValue & 2) != 0)
                {
                    cornerValue += 2; // Top-Right
                }

                if (((_chunkMap[x + 1, y - 1, z] != 0)
                     || (isNz && _chunkMap[x + 1, y - 1, z - 1] != 0))
                    && (edgeValue & 2) != 0 && (edgeValue & 4) != 0)
                {
                    cornerValue += 4; // Bottom-Right
                }

                if (((_chunkMap[x - 1, y - 1, z] != 0)
                     || (isNz && _chunkMap[x - 1, y - 1, z - 1] != 0))
                    && (edgeValue & 4) != 0 && (edgeValue & 8) != 0)
                {
                    cornerValue += 8; // Bottom-Left
                }
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
            
            if (mode != dir.py){
                spriteNumber += 48;
            }
            return spriteNumber;
        }
 
    }
 
 

    public int GetBlockInChunk(Vector3Int chunkCoordinate, Vector3Int blockCoordinate, WorldStatic worldStatic) //0 = empty, 1 = block, error = out of bounds
    {
        try
        { 
            var chunk = worldStatic.GetChunk(chunkCoordinate);
            if (chunk != null)
            {
                return chunk.Map[blockCoordinate.x, blockCoordinate.y, blockCoordinate.z];
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



 


public struct NativeMap3D<T> where T : struct
{
    private NativeArray<T> array;
    private int size;

    public NativeMap3D(int size, Allocator allocator)
    {
        this.size = size;
        this.array = new NativeArray<T>(size * size * size, allocator);
    }

    public T this[int x, int y, int z]
    {
        get => array[(x+1) * size * size + (y+1) * size + (z+1)];
        set => array[(x+1) * size * size + (y+1) * size + (z+1)] = value;
    }

    public void Dispose()
    {
        array.Dispose();
    }
}
