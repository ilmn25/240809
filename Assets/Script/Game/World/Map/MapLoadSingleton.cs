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
using UnityEngine.Rendering;

// using Unity.Burst;

public class MapLoadSingleton : MonoBehaviour
         
{
    public static MapLoadSingleton Instance { get; private set; }
 

    private void OnApplicationQuit()
    {
        _colx.Dispose();
        _rowy.Dispose();
        _textureRectDictionary.Dispose();
    }
    
    private async void Start()
    {   
        Instance = this;
        WorldSingleton.PlayerChunkTraverse += HandleChunkMapTraverse;

        _tileSize = 16;
        _tilesPerRow = 12;
        int[] colx = new int[] {0, 16, 32, 48, 64, 80, 96, 112, 128, 144, 160, 176};
        int[] rowy = new int[] {112, 96, 80, 64, 48, 32, 16, 0};
        _colx = new NativeArray<int>(colx, Allocator.Persistent);
        _rowy = new NativeArray<int>(rowy, Allocator.Persistent);
        
        _textureRectDictionary = new NativeHashMap<int, Rect>(BlockSingleton.TextureRectDictionary.Count, Allocator.Persistent);
        foreach (var kvp in BlockSingleton.TextureRectDictionary)
        {
            _textureRectDictionary[kvp.Key] = kvp.Value;
        }
        
        
        
        await Task.Delay(50);
        HandleChunkMapTraverse(); 
        WorldSingleton.Instance.GenerateBoolMap();
    }

    
    public void RefreshExistingChunk(Vector3Int chunkCoordinates)
    {
        if (!WorldSingleton.Instance.IsInWorldBounds(chunkCoordinates)) return;
        _ = LoadChunksOntoScreenAsync(chunkCoordinates, true);
    }
     

    private Vector3Int _traverseCheckPosition;
    private List<Vector3Int> _destroyList = new List<Vector3Int>();
    void HandleChunkMapTraverse()
    {
        foreach (var kvp in _activeChunks)
        {
            _traverseCheckPosition = kvp.Key;
            if (!WorldSingleton.InPlayerRange(kvp.Key, WorldSingleton.RENDER_DISTANCE))
            {
                Destroy(kvp.Value.gameObject, 1);
                EntityStaticLoadSingleton.Instance.UnloadEntitiesInChunk(kvp.Key); //static entities load in 
                EntityStaticLoadSingleton._activeEntities.Remove(kvp.Key);
                _destroyList.Add(kvp.Key);
            }
        }

        // Remove the keys after iteration to avoid modifying the collection while iterating
        foreach (var key in _destroyList)
        {
            _activeChunks.Remove(key);
        }
        _destroyList.Clear();

        // Collect chunk coordinates within render distance
        for (int x = -WorldSingleton.RENDER_RANGE; x <= WorldSingleton.RENDER_RANGE; x++)
        {
            for (int y = -WorldSingleton.RENDER_RANGE; y <= WorldSingleton.RENDER_RANGE; y++)
            {
                for (int z = -WorldSingleton.RENDER_RANGE; z <= WorldSingleton.RENDER_RANGE; z++)
                {
                    _traverseCheckPosition = new Vector3Int(
                        WorldSingleton._playerChunkPos.x + x * WorldSingleton.CHUNK_SIZE,
                        WorldSingleton._playerChunkPos.y + y * WorldSingleton.CHUNK_SIZE,
                        WorldSingleton._playerChunkPos.z + z * WorldSingleton.CHUNK_SIZE
                    );
                    if (!_activeChunks.ContainsKey(_traverseCheckPosition) && WorldSingleton.Instance.IsInWorldBounds(_traverseCheckPosition))
                        _ = LoadChunksOntoScreenAsync(_traverseCheckPosition);
                }
            }
        } 
    }

    public Dictionary<Vector3Int, MapCullModule> _activeChunks = new Dictionary<Vector3Int, MapCullModule>();
    private SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1); 
    private async Task LoadChunksOntoScreenAsync(Vector3Int chunkCoord, bool replace = false)
    { 
        await _semaphoreSlim.WaitAsync();
        try
        { 
            if ((replace || !_activeChunks.ContainsKey(chunkCoord)) && WorldSingleton.InPlayerRange(chunkCoord, WorldSingleton.RENDER_DISTANCE))
            {
                _chunkCoordinate = chunkCoord;
                _chunkData = WorldSingleton.World[chunkCoord.x, chunkCoord.y, chunkCoord.z];
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
    private MapCullModule _mapCullModule;
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
            _meshObject.layer = Game.IndexMap;
            _meshObject.transform.position = _chunkCoordinate; 

            _meshObject.AddComponent<MeshFilter>();
            _meshRenderer = _meshObject.AddComponent<MeshRenderer>();  
            _meshRenderer.material = BlockSingleton.MeshMaterial; 

            _mapCullModule = _meshObject.AddComponent<MapCullModule>();  
            _mapCullModule._meshData = _mesh;
            _mapCullModule._verticesShadow = _verticesShadow;
            _mapCullModule._count = _count;
            _activeChunks.Add(_chunkCoordinate, _mapCullModule);
        } 
        else 
        {
            _meshObject = _activeChunks[_chunkCoordinate].gameObject;
            _meshRenderer = _meshObject.GetComponent<MeshRenderer>(); 
            _meshRenderer.material = BlockSingleton.MeshMaterial; 

            _mapCullModule = _meshObject.GetComponent<MapCullModule>();  
            _mapCullModule._meshData = _mesh;
            _mapCullModule._verticesShadow = _verticesShadow;
            _mapCullModule._count = _count;
            _mapCullModule.HandleAssignment(); 
        } 
    }
 



    // const
    private int _tileSize;
    private int _tilesPerRow;
    private NativeArray<int> _colx;
    private NativeArray<int> _rowy; 
    private NativeHashMap<int, Rect> _textureRectDictionary;

    // input
    private Vector3Int _chunkCoordinate;
    private ChunkData _chunkData;  

    // output
    private List<Vector3> _vertices;
    private List<Vector3> _verticesShadow;
    private List<int> _triangles;
    private List<Vector2> _uvs; 
    private List<Vector3> _normals;
    private int[] _count; 
    private void LoadMeshMath()
    { 
        try
        {    
            // Initialize local temp arrays 
            MeshMathJob job = new MeshMathJob
            {
                // const
                chunkSize = WorldSingleton.CHUNK_SIZE,
                tileSize = _tileSize,
                tilesPerRow = _tilesPerRow, 
                colx = _colx,
                rowy = _rowy,  
                textureRectDictionary = _textureRectDictionary,
                textureAtlasWidth = BlockSingleton.TextureAtlasWidth,
                textureAtlasHeight = BlockSingleton.TextureAtlasHeight, 

                // input 
                mapLoadData = MapLoadData.Create(_chunkCoordinate),

                // output
                vertices = new NativeList<Vector3>(Allocator.TempJob),
                verticesShadow = new NativeList<Vector3>(Allocator.TempJob),
                triangles = new NativeList<int>(Allocator.TempJob),
                uvs = new NativeList<Vector2>(Allocator.TempJob),
                normals = new NativeList<Vector3>(Allocator.TempJob),
                count = new NativeList<int>(Allocator.TempJob),
                
                // local temp
                faceVertices = new NativeArray<Vector3>(4, Allocator.TempJob),
                faceVerticesShadow = new NativeArray<Vector3>(4, Allocator.TempJob),
                spriteUVs = new NativeArray<Vector2>(4, Allocator.TempJob)
            };

            // Schedule the job
            JobHandle jobHandle = job.Schedule();

            jobHandle.Complete();

            _vertices = new List<Vector3>(job.vertices.ToArray());
            _verticesShadow = new List<Vector3>(job.verticesShadow.ToArray());
            _triangles = new List<int>(job.triangles.ToArray());
            _uvs = new List<Vector2>(job.uvs.ToArray());
            _normals = new List<Vector3>(job.normals.ToArray());   
            _count = job.count.ToArray();
            
            // Dispose of the native arrays and lists 
            job.vertices.Dispose();
            job.verticesShadow.Dispose();
            job.triangles.Dispose();
            job.uvs.Dispose();
            job.normals.Dispose();
            job.count.Dispose();
             
        }
        catch (Exception ex)
        {
            Debug.LogError($"An exception occurred in MeshMathJob: {ex.Message}");
        } 
    }
         

    enum dir { px, nx, py, ny, pz, nz}
    // [BurstCompile(CompileSynchronously = true)]
    public struct MeshMathJob : IJob
    {
        // const
        [DeallocateOnJobCompletion]
        public int chunkSize; 
        [DeallocateOnJobCompletion]
        public int tileSize;
        [DeallocateOnJobCompletion]
        public int tilesPerRow; 
        public NativeArray<int> colx; 
        public NativeArray<int> rowy;    
        public NativeHashMap<int, Rect> textureRectDictionary;
        
        [DeallocateOnJobCompletion]
        public int textureAtlasWidth;
        [DeallocateOnJobCompletion]
        public int textureAtlasHeight; 

        // input 
        [DeallocateOnJobCompletion]
        public NativeMap3D<int> mapLoadData;  

        // output
        public NativeList<Vector3> vertices;
        public NativeList<Vector3> verticesShadow;
        public NativeList<int> triangles;
        public NativeList<Vector2> uvs;
        public NativeList<Vector3> normals;
        public NativeList<int> count; 

        // local temp
        [DeallocateOnJobCompletion]
        private int blockID;
        [DeallocateOnJobCompletion] 
        private Vector3Int blockPosition;

        
        [DeallocateOnJobCompletion]
        public NativeArray<Vector3> faceVertices;
        [DeallocateOnJobCompletion]
        public NativeArray<Vector3> faceVerticesShadow;
        [DeallocateOnJobCompletion] 
        private Vector3 normal;
        [DeallocateOnJobCompletion]
        public NativeArray<Vector2> spriteUVs; 
        [DeallocateOnJobCompletion]
        private int spriteNumber;
        [DeallocateOnJobCompletion]
        private int edgeValue;
        [DeallocateOnJobCompletion]
        private int cornerValue;  
        [DeallocateOnJobCompletion] 
        private Rect textureRect;
  
        public void Execute()
        {   
            for (int y = 0; y < chunkSize; y++)
            {
                count.Add(y == 0? 0 : count[y-1]);
                for (int z = 0; z < chunkSize; z++)
                {
                    for (int x = 0; x < chunkSize; x++)
                    { 
                        blockID = mapLoadData[x, y, z];

                        if (blockID != 0)
                        {
                            blockPosition = new Vector3Int(x, y, z);

                            if (mapLoadData[x, y + 1, z] == 0) HandleMeshFace(dir.py, HandleMeshAutoTile(x, y, z, dir.py)); // Top
                            if (mapLoadData[x, y - 1, z] == 0) HandleMeshFace(dir.ny, 1); // Bottom
                            if (mapLoadData[x + 1, y, z] == 0) HandleMeshFace(dir.px, HandleMeshAutoTile(x, y, z, dir.px)); // Right
                            if (mapLoadData[x - 1, y, z] == 0) HandleMeshFace(dir.nx, HandleMeshAutoTile(x, y, z, dir.nx)); // Left
                            if (mapLoadData[x, y, z + 1] == 0) HandleMeshFace(dir.pz, HandleMeshAutoTile(x, y, z, dir.pz)); // Front
                            if (mapLoadData[x, y, z - 1] == 0) HandleMeshFace(dir.nz, HandleMeshAutoTile(x, y, z, dir.nz)); // Back
                        }
                    }
                }
 
            }
        }

        void HandleMeshFace(dir direction, int textureIndex)
        {
            count[blockPosition.y]++;
            int vertexIndex = vertices.Length; 
            normal = Vector3.zero;

            if (direction == dir.py) // top
            {
                faceVertices[0] = blockPosition + new Vector3(0, 1, 0);
                faceVertices[1] = blockPosition + new Vector3(1, 1, 0);
                faceVertices[2] = blockPosition + new Vector3(1, 1, 1);
                faceVertices[3] = blockPosition + new Vector3(0, 1, 1);

                faceVerticesShadow[0] = blockPosition + new Vector3(-0.03f, 1, -0.03f);
                faceVerticesShadow[1] = blockPosition + new Vector3(1.03f, 1, -0.03f);
                faceVerticesShadow[2] = blockPosition + new Vector3(1.03f, 1, 1.03f);
                faceVerticesShadow[3] = blockPosition + new Vector3(-0.03f, 1, 1.03f);
                 
                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 3);
                triangles.Add(vertexIndex + 2);
                normal = new Vector3(0, 1, 0);
            } else if (direction == dir.ny) // down
            {
                faceVertices[0] = blockPosition + new Vector3(0, 0, 1);
                faceVertices[1] = blockPosition + new Vector3(1, 0, 1);
                faceVertices[2] = blockPosition + new Vector3(1, 0, 0);
                faceVertices[3] = blockPosition + new Vector3(0, 0, 0);
                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 3);
                triangles.Add(vertexIndex + 2);
                normal = new Vector3(0, -1, 0);
            }
            else if (direction == dir.pz) // back
            {
                faceVertices[0] = blockPosition + new Vector3(0, 0, 1);
                faceVertices[1] = blockPosition + new Vector3(1, 0, 1);
                faceVertices[2] = blockPosition + new Vector3(1, 1, 1);
                faceVertices[3] = blockPosition + new Vector3(0, 1, 1);
                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 3);
                normal = new Vector3(0, 0, 1);
            }
            else if (direction == dir.nz) // front
            {
                faceVertices[0] = blockPosition + new Vector3(0, 0, 0);
                faceVertices[1] = blockPosition + new Vector3(1, 0, 0);
                faceVertices[2] = blockPosition + new Vector3(1, 1, 0);
                faceVertices[3] = blockPosition + new Vector3(0, 1, 0);
                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 3);
                triangles.Add(vertexIndex + 2);
                normal = new Vector3(0, 0, -1);
            }
            else if (direction == dir.nx) // left
            {
                faceVertices[0] = blockPosition + new Vector3(0, 0, 0);
                faceVertices[1] = blockPosition + new Vector3(0, 0, 1);
                faceVertices[2] = blockPosition + new Vector3(0, 1, 1);
                faceVertices[3] = blockPosition + new Vector3(0, 1, 0);
                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 3);
                normal = new Vector3(-1, 0, 0);
            }
            else if (direction == dir.px) // right
            {
                faceVertices[0] = blockPosition + new Vector3(1, 0, 0);
                faceVertices[1] = blockPosition + new Vector3(1, 0, 1);
                faceVertices[2] = blockPosition + new Vector3(1, 1, 1);
                faceVertices[3] = blockPosition + new Vector3(1, 1, 0);
                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 3);
                triangles.Add(vertexIndex + 2);
                normal = new Vector3(1, 0, 0);
            }
 
 
            Vector2Int tile = GetTileRect(textureIndex);
            Vector2[] _spriteUVs = new Vector2[]
            {
                new Vector2(tile.x, tile.y),
                new Vector2(tile.x + tileSize, tile.y),
                new Vector2(tile.x + tileSize, tile.y + tileSize),
                new Vector2(tile.x, tile.y + tileSize)
            };

            textureRect = textureRectDictionary[blockID];

            // Calculate the new UVs based on the original rect's position and size
            for (int i = 0; i < _spriteUVs.Length; i++)
            {
                _spriteUVs[i] = new Vector2(
                    (_spriteUVs[i].x / textureAtlasWidth) + textureRect.x,
                    (_spriteUVs[i].y / textureAtlasHeight) + textureRect.y
                );
            }
 
            // Add each UV coordinate individually
            for (int i = 0; i < _spriteUVs.Length; i++)
            {
                uvs.Add(_spriteUVs[i]);
                normals.Add(normal);
                vertices.Add(faceVertices[i]);

                if (direction == dir.py) 
                {
                    verticesShadow.Add(faceVerticesShadow[i]);
                }
                else
                {
                    verticesShadow.Add(faceVertices[i]);
                }
            } 
        }

        Vector2Int GetTileRect(int index)
        { 
            int targetRow = index / tilesPerRow;
            int targetCol = index % tilesPerRow;  

            return new Vector2Int(colx[targetCol], rowy[targetRow]);
        }
          

        int HandleMeshAutoTile(int x, int y, int z, dir mode)
        {
            spriteNumber = 0;
            edgeValue = 0;
            cornerValue = 0;

            if (mode == dir.py) // Top
            {
                bool isPy = mapLoadData[x, y + 1, z] == 0;

                if ((mapLoadData[x, y, z + 1] != 0)
                    || (isPy && mapLoadData[x, y + 1, z + 1] != 0))
                {
                    edgeValue += 1; // Top
                }

                if ((mapLoadData[x + 1, y, z] != 0)
                    || (isPy && mapLoadData[x + 1, y + 1, z] != 0))
                {
                    edgeValue += 2; // Right
                }

                if ((mapLoadData[x, y, z - 1] != 0)
                    || (isPy && mapLoadData[x, y + 1, z - 1] != 0))
                {
                    edgeValue += 4; // Bottom
                }

                if ((mapLoadData[x - 1, y, z] != 0)
                    || (isPy && mapLoadData[x - 1, y + 1, z] != 0))
                {
                    edgeValue += 8; // Left
                }

                // Calculate corner values
                if (((mapLoadData[x - 1, y, z + 1] != 0)
                    || (isPy && mapLoadData[x - 1, y + 1, z + 1] != 0))
                    && (edgeValue & 1) != 0 && (edgeValue & 8) != 0)
                {
                    cornerValue += 1; // Top-Left
                }

                if (((mapLoadData[x + 1, y, z + 1] != 0)
                    || (isPy && mapLoadData[x + 1, y + 1, z + 1] != 0))
                    && (edgeValue & 1) != 0 && (edgeValue & 2) != 0)
                {
                    cornerValue += 2; // Top-Right
                }

                if (((mapLoadData[x + 1, y, z - 1] != 0)
                    || (isPy && mapLoadData[x + 1, y + 1, z - 1] != 0))
                    && (edgeValue & 2) != 0 && (edgeValue & 4) != 0)
                {
                    cornerValue += 4; // Bottom-Right
                }

                if (((mapLoadData[x - 1, y, z - 1] != 0)
                    || (isPy && mapLoadData[x - 1, y + 1, z - 1] != 0))
                    && (edgeValue & 4) != 0 && (edgeValue & 8) != 0)
                {
                    cornerValue += 8; // Bottom-Left
                }
            }  
            else if (mode == dir.nx) // Negative X (Left side of the cube)
            {
                bool isNx = mapLoadData[x - 1, y, z] == 0;

                if ((mapLoadData[x, y + 1, z] != 0)
                    || (isNx && mapLoadData[x - 1, y + 1, z] != 0))
                {
                    edgeValue += 1; // Top
                }

                if ((mapLoadData[x, y, z + 1] != 0)
                    || (isNx && mapLoadData[x - 1, y, z + 1] != 0))
                {
                    edgeValue += 2; // Right
                }

                if ((mapLoadData[x, y - 1, z] != 0)
                    || (isNx && mapLoadData[x - 1, y - 1, z] != 0))
                {
                    edgeValue += 4; // Bottom
                }

                if ((mapLoadData[x, y, z - 1] != 0)
                    || (isNx && mapLoadData[x - 1, y, z - 1] != 0))
                {
                    edgeValue += 8; // Left
                }

                // Calculate corner values
                
                if (((mapLoadData[x, y + 1, z - 1] != 0)
                     || (isNx && mapLoadData[x - 1, y + 1, z - 1] != 0))
                    && (edgeValue & 1) != 0 && (edgeValue & 8) != 0)
                {
                    cornerValue += 1; // Top-Left
                }
                if (((mapLoadData[x, y + 1, z + 1] != 0)
                     || (isNx && mapLoadData[x - 1, y + 1, z + 1] != 0))
                    && (edgeValue & 1) != 0 && (edgeValue & 2) != 0)
                {
                    cornerValue += 2; // Top-Right
                }

                if (((mapLoadData[x, y - 1, z + 1] != 0)
                     || (isNx && mapLoadData[x - 1, y - 1, z + 1] != 0))
                    && (edgeValue & 2) != 0 && (edgeValue & 4) != 0)
                {
                    cornerValue += 4; // Bottom-Right
                }

                if (((mapLoadData[x, y - 1, z - 1] != 0)
                     || (isNx && mapLoadData[x - 1, y - 1, z - 1] != 0))
                    && (edgeValue & 4) != 0 && (edgeValue & 8) != 0)
                {
                    cornerValue += 8; // Bottom-Left
                } 
            }
            else if (mode == dir.px) // Positive X (Right side of the cube)
            {
                bool isPx = mapLoadData[x + 1, y, z] == 0;

                if ((mapLoadData[x, y + 1, z] != 0)
                    || (isPx && mapLoadData[x + 1, y + 1, z] != 0))
                {
                    edgeValue += 1; // Top
                }

                if ((mapLoadData[x, y, z + 1] != 0)
                    || (isPx && mapLoadData[x + 1, y, z + 1] != 0))
                {
                    edgeValue += 2; // Right
                }

                if ((mapLoadData[x, y - 1, z] != 0)
                    || (isPx && mapLoadData[x + 1, y - 1, z] != 0))
                {
                    edgeValue += 4; // Bottom
                }

                if ((mapLoadData[x, y, z - 1] != 0)
                    || (isPx && mapLoadData[x + 1, y, z - 1] != 0))
                {
                    edgeValue += 8; // Left
                }

                // Lib.Log(x,y,z,_chunkMap[x, y + 1, z] != 0, _chunkMap[x, y, z + 1] != 0,_chunkMap[x, y - 1, z] != 0, _chunkMap[x, y, z - 1] != 0, _chunkMap[x, y + 1, z + 1] != 0, _chunkMap[x, y - 1, z + 1] != 0, _chunkMap[x, y - 1, z - 1] != 0, _chunkMap[x, y + 1, z - 1] != 0);
                // Calculate corner values

                if (((mapLoadData[x, y + 1, z - 1] != 0)
                     || (isPx && mapLoadData[x + 1, y + 1, z - 1] != 0))
                    && (edgeValue & 1) != 0 && (edgeValue & 8) != 0)
                {
                    cornerValue += 1; // Top-Left
                }
                
                if (((mapLoadData[x, y + 1, z + 1] != 0)
                     || (isPx && mapLoadData[x + 1, y + 1, z + 1] != 0))
                    && (edgeValue & 1) != 0 && (edgeValue & 2) != 0)
                {
                    cornerValue += 2; // Top-Right
                }

                if (((mapLoadData[x, y - 1, z + 1] != 0)
                     || (isPx && mapLoadData[x + 1, y - 1, z + 1] != 0))
                    && (edgeValue & 2) != 0 && (edgeValue & 4) != 0)
                {
                    cornerValue += 4; // Bottom-Right
                }

                if (((mapLoadData[x, y - 1, z - 1] != 0)
                     || (isPx && mapLoadData[x + 1, y - 1, z - 1] != 0))
                    && (edgeValue & 4) != 0 && (edgeValue & 8) != 0)
                {
                    cornerValue += 8; // Bottom-Left
                } 
            }
            else if (mode == dir.pz) // Positive Z (Front of the cube)
            {
                bool isPz = mapLoadData[x, y, z + 1] == 0;

                if ((mapLoadData[x, y + 1, z] != 0)
                    || (isPz && mapLoadData[x, y + 1, z + 1] != 0))
                {
                    edgeValue += 1; // Top
                }

                if ((mapLoadData[x + 1, y, z] != 0)
                    || (isPz && mapLoadData[x + 1, y, z + 1] != 0))
                {
                    edgeValue += 2; // Right
                }

                if ((mapLoadData[x, y - 1, z] != 0)
                    || (isPz && mapLoadData[x, y - 1, z + 1] != 0))
                {
                    edgeValue += 4; // Bottom
                }

                if ((mapLoadData[x - 1, y, z] != 0)
                    || (isPz && mapLoadData[x - 1, y, z + 1] != 0))
                {
                    edgeValue += 8; // Left
                }

                // Calculate corner values
                if (((mapLoadData[x - 1, y + 1, z] != 0)
                     || (isPz && mapLoadData[x - 1, y + 1, z + 1] != 0))
                    && (edgeValue & 1) != 0 && (edgeValue & 8) != 0)
                {
                    cornerValue += 1; // Top-Left
                }

                if (((mapLoadData[x + 1, y + 1, z] != 0)
                     || (isPz && mapLoadData[x + 1, y + 1, z + 1] != 0))
                    && (edgeValue & 1) != 0 && (edgeValue & 2) != 0)
                {
                    cornerValue += 2; // Top-Right
                }

                if (((mapLoadData[x + 1, y - 1, z] != 0)
                     || (isPz && mapLoadData[x + 1, y - 1, z + 1] != 0))
                    && (edgeValue & 2) != 0 && (edgeValue & 4) != 0)
                {
                    cornerValue += 4; // Bottom-Right
                }

                if (((mapLoadData[x - 1, y - 1, z] != 0)
                     || (isPz && mapLoadData[x - 1, y - 1, z + 1] != 0))
                    && (edgeValue & 4) != 0 && (edgeValue & 8) != 0)
                {
                    cornerValue += 8; // Bottom-Left
                }
            }
            else if (mode == dir.nz) // Negative Z (Back of the cube)
            {
                bool isNz = mapLoadData[x, y, z - 1] == 0;

                if ((mapLoadData[x, y + 1, z] != 0)
                    || (isNz && mapLoadData[x, y + 1, z - 1] != 0))
                {
                    edgeValue += 1; // Top
                }

                if ((mapLoadData[x + 1, y, z] != 0)
                    || (isNz && mapLoadData[x + 1, y, z - 1] != 0))
                {
                    edgeValue += 2; // Right
                }

                if ((mapLoadData[x, y - 1, z] != 0)
                    || (isNz && mapLoadData[x, y - 1, z - 1] != 0))
                {
                    edgeValue += 4; // Bottom
                }

                if ((mapLoadData[x - 1, y, z] != 0)
                    || (isNz && mapLoadData[x - 1, y, z - 1] != 0))
                {
                    edgeValue += 8; // Left
                }

                // Calculate corner values
                if (((mapLoadData[x - 1, y + 1, z] != 0)
                     || (isNz && mapLoadData[x - 1, y + 1, z - 1] != 0))
                    && (edgeValue & 1) != 0 && (edgeValue & 8) != 0)
                {
                    cornerValue += 1; // Top-Left
                }

                if (((mapLoadData[x + 1, y + 1, z] != 0)
                     || (isNz && mapLoadData[x + 1, y + 1, z - 1] != 0))
                    && (edgeValue & 1) != 0 && (edgeValue & 2) != 0)
                {
                    cornerValue += 2; // Top-Right
                }

                if (((mapLoadData[x + 1, y - 1, z] != 0)
                     || (isNz && mapLoadData[x + 1, y - 1, z - 1] != 0))
                    && (edgeValue & 2) != 0 && (edgeValue & 4) != 0)
                {
                    cornerValue += 4; // Bottom-Right
                }

                if (((mapLoadData[x - 1, y - 1, z] != 0)
                     || (isNz && mapLoadData[x - 1, y - 1, z - 1] != 0))
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
 
 

    public int GetBlockInChunk(Vector3Int coordinate) //0 = empty, 1 = block, error = out of bounds
    {
        Vector3Int blockCoordinate = WorldSingleton.GetBlockCoordinate(coordinate);
        Vector3Int chunkCoordinate = WorldSingleton.GetChunkCoordinate(coordinate); 
        ChunkData chunk = WorldSingleton.Instance.GetChunk(chunkCoordinate);
        if (chunk != null)
        {
            return chunk.Map[blockCoordinate.x, blockCoordinate.y, blockCoordinate.z];
        }
        return 0; 
    }
}



 
 