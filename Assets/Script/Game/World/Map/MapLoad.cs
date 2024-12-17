using System.Collections.Generic;
using UnityEngine; 
using System.Threading.Tasks;
using System.Threading;
using System;
using Unity.Jobs;
using Unity.Collections;
using Object = UnityEngine.Object;


public class MapLoad
{
    private enum Dir { Px, Nx, Py, Ny, Pz, Nz}
    public static Dictionary<Vector3Int, MapCullModule> ActiveChunks = new Dictionary<Vector3Int, MapCullModule>();
    private static SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1); 
    
    private static Vector3Int _traverseCheckPosition;
    private static List<Vector3Int> _destroyList = new List<Vector3Int>();
    
    private static Mesh _mesh; 
    private static GameObject _meshObject;
    private static MeshRenderer _meshRenderer;
    private static MapCullModule _mapCullModule;
    
    // input
    private static Vector3Int _chunkCoordinate;
    private static ChunkData _chunkData;  

    // output
    private static List<Vector3> _vertices;
    private static List<Vector3> _verticesShadow;
    private static List<int> _triangles;
    private static List<Vector2> _uvs; 
    private static List<Vector3> _normals;
    private static int[] _count; 
    
    public static async void Initialize()
    {   
        Scene.PlayerChunkTraverse += HandleChunkMapTraverse;
        
        await Task.Delay(50);
        HandleChunkMapTraverse(); 
        NavMap.GenerateNavMap();
    }
    
    public static void RefreshExistingChunk(Vector3Int chunkCoordinates)
    {
        if (!World.IsInWorldBounds(chunkCoordinates)) return;
        _ = LoadChunksOntoScreenAsync(chunkCoordinates, true);
    }

    static void HandleChunkMapTraverse()
    {
        foreach (var kvp in ActiveChunks)
        {
            if (!Scene.InPlayerChunkRange(kvp.Key, Scene.RenderDistance))
            {
                Object.Destroy(kvp.Value.gameObject, 1);
                EntityStaticLoad.UnloadEntitiesInChunk(kvp.Key); //static entities load in 
                EntityStaticLoad._activeEntities.Remove(kvp.Key);
                _destroyList.Add(kvp.Key);
            }
        }

        foreach (var key in _destroyList)
        {
            ActiveChunks.Remove(key);
        }
        _destroyList.Clear();

        for (int x = -Scene.RenderRange; x <= Scene.RenderRange; x++)
        {
            for (int y = -Scene.RenderRange; y <= Scene.RenderRange; y++)
            {
                for (int z = -Scene.RenderRange; z <= Scene.RenderRange; z++)
                {
                    _traverseCheckPosition = new Vector3Int(
                        Scene.PlayerChunkPosition.x + x * World.ChunkSize,
                        Scene.PlayerChunkPosition.y + y * World.ChunkSize,
                        Scene.PlayerChunkPosition.z + z * World.ChunkSize
                    );
                    if (!ActiveChunks.ContainsKey(_traverseCheckPosition) && World.IsInWorldBounds(_traverseCheckPosition))
                        _ = LoadChunksOntoScreenAsync(_traverseCheckPosition);
                }
            }
        } 
    }
 
    private static async Task LoadChunksOntoScreenAsync(Vector3Int chunkCoord, bool replace = false)
    { 
        await _semaphoreSlim.WaitAsync();
        try
        { 
            if ((replace || !ActiveChunks.ContainsKey(chunkCoord)) && Scene.InPlayerChunkRange(chunkCoord, Scene.RenderDistance))
            {
                _chunkCoordinate = chunkCoord;
                _chunkData = World.world[chunkCoord.x, chunkCoord.y, chunkCoord.z];
                if (_chunkData != ChunkData.Zero)
                {
                    await Task.Run(() => LoadMeshMath()); 
                    await Task.Delay(10);
                    LoadMeshObject(replace);
                }  else Utility.Log("Chunk in queue is zero");
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
 
    private static void LoadMeshObject(bool replace = false)
    {
        _mesh = new Mesh();
        _mesh.SetVertices(_vertices);
        _mesh.SetTriangles(_triangles, 0);
        _mesh.SetUVs(0, _uvs);
        _mesh.SetNormals(_normals); 
 
        if (!replace)
        {
            _meshObject = new("chunk");
            _meshObject.layer = Game.IndexMap;
            _meshObject.transform.position = _chunkCoordinate; 

            _meshObject.AddComponent<MeshFilter>();
            _meshRenderer = _meshObject.AddComponent<MeshRenderer>();  
            _meshRenderer.material = Block.MeshMaterial; 

            _mapCullModule = _meshObject.AddComponent<MapCullModule>();  
            _mapCullModule._meshData = _mesh;
            _mapCullModule._verticesShadow = _verticesShadow;
            _mapCullModule._count = _count;
            ActiveChunks.Add(_chunkCoordinate, _mapCullModule);
        } 
        else 
        {
            _meshObject = ActiveChunks[_chunkCoordinate].gameObject;
            _meshRenderer = _meshObject.GetComponent<MeshRenderer>(); 
            _meshRenderer.material = Block.MeshMaterial; 

            _mapCullModule = _meshObject.GetComponent<MapCullModule>();  
            _mapCullModule._meshData = _mesh;
            _mapCullModule._verticesShadow = _verticesShadow;
            _mapCullModule._count = _count;
            _mapCullModule.HandleAssignment(); 
        } 
    }
 
    private static void LoadMeshMath()
    { 
        try
        {    
            // Initialize local temp arrays 
            MeshMathJob job = new MeshMathJob
            {
                // const
                ChunkSize = World.ChunkSize,
                TileSize = Block.TileSize,
                TilesPerRow = Block.TilesPerRow, 
                Colx = Block.Colx,
                Rowy = Block.Rowy,  
                TextureRectDictionary = Block.TextureRectDictionary,
                TextureAtlasWidth = Block.TextureAtlasWidth,
                TextureAtlasHeight = Block.TextureAtlasHeight, 

                // input 
                MapLoadData = MapLoadData.Create(_chunkCoordinate),

                // output
                Vertices = new NativeList<Vector3>(Allocator.TempJob),
                VerticesShadow = new NativeList<Vector3>(Allocator.TempJob),
                Triangles = new NativeList<int>(Allocator.TempJob),
                Uvs = new NativeList<Vector2>(Allocator.TempJob),
                Normals = new NativeList<Vector3>(Allocator.TempJob),
                Count = new NativeList<int>(Allocator.TempJob),
                
                // local temp
                FaceVertices = new NativeArray<Vector3>(4, Allocator.TempJob),
                FaceVerticesShadow = new NativeArray<Vector3>(4, Allocator.TempJob),
                SpriteUVs = new NativeArray<Vector2>(4, Allocator.TempJob)
            };

            // Schedule the job
            JobHandle jobHandle = job.Schedule();

            jobHandle.Complete();

            _vertices = new List<Vector3>(job.Vertices.ToArray());
            _verticesShadow = new List<Vector3>(job.VerticesShadow.ToArray());
            _triangles = new List<int>(job.Triangles.ToArray());
            _uvs = new List<Vector2>(job.Uvs.ToArray());
            _normals = new List<Vector3>(job.Normals.ToArray());   
            _count = job.Count.ToArray();
            
            // Dispose of the native arrays and lists 
            job.Vertices.Dispose();
            job.VerticesShadow.Dispose();
            job.Triangles.Dispose();
            job.Uvs.Dispose();
            job.Normals.Dispose();
            job.Count.Dispose();
             
        }
        catch (Exception ex)
        {
            Debug.LogError($"An exception occurred in MeshMathJob: {ex.Message}");
        } 
    }
          
    public struct MeshMathJob : IJob
    {
        // const
        [DeallocateOnJobCompletion]
        public int ChunkSize; 
        [DeallocateOnJobCompletion]
        public int TileSize;
        [DeallocateOnJobCompletion]
        public int TilesPerRow; 
        public NativeArray<int> Colx; 
        public NativeArray<int> Rowy;    
        public NativeHashMap<int, Rect> TextureRectDictionary;
        
        [DeallocateOnJobCompletion]
        public int TextureAtlasWidth;
        [DeallocateOnJobCompletion]
        public int TextureAtlasHeight; 

        // input 
        [DeallocateOnJobCompletion]
        public NativeMap3D<int> MapLoadData;  

        // output
        public NativeList<Vector3> Vertices;
        public NativeList<Vector3> VerticesShadow;
        public NativeList<int> Triangles;
        public NativeList<Vector2> Uvs;
        public NativeList<Vector3> Normals;
        public NativeList<int> Count; 

        // local temp
        [DeallocateOnJobCompletion]
        private int _blockID;
        [DeallocateOnJobCompletion] 
        private Vector3Int _blockPosition;
        
        [DeallocateOnJobCompletion]
        public NativeArray<Vector3> FaceVertices;
        [DeallocateOnJobCompletion]
        public NativeArray<Vector3> FaceVerticesShadow;
        [DeallocateOnJobCompletion] 
        private Vector3 _normal;
        [DeallocateOnJobCompletion]
        public NativeArray<Vector2> SpriteUVs; 
        [DeallocateOnJobCompletion]
        private int _spriteNumber;
        [DeallocateOnJobCompletion]
        private int _edgeValue;
        [DeallocateOnJobCompletion]
        private int _cornerValue;  
        [DeallocateOnJobCompletion] 
        private Rect _textureRect;
  
        public void Execute()
        {   
            for (int y = 0; y < ChunkSize; y++)
            {
                Count.Add(y == 0? 0 : Count[y-1]);
                for (int z = 0; z < ChunkSize; z++)
                {
                    for (int x = 0; x < ChunkSize; x++)
                    { 
                        _blockID = MapLoadData[x, y, z];

                        if (_blockID != 0)
                        {
                            _blockPosition = new Vector3Int(x, y, z);

                            if (MapLoadData[x, y + 1, z] == 0) HandleMeshFace(Dir.Py, HandleMeshAutoTile(x, y, z, Dir.Py)); // Top
                            if (MapLoadData[x, y - 1, z] == 0) HandleMeshFace(Dir.Ny, 1); // Bottom
                            if (MapLoadData[x + 1, y, z] == 0) HandleMeshFace(Dir.Px, HandleMeshAutoTile(x, y, z, Dir.Px)); // Right
                            if (MapLoadData[x - 1, y, z] == 0) HandleMeshFace(Dir.Nx, HandleMeshAutoTile(x, y, z, Dir.Nx)); // Left
                            if (MapLoadData[x, y, z + 1] == 0) HandleMeshFace(Dir.Pz, HandleMeshAutoTile(x, y, z, Dir.Pz)); // Front
                            if (MapLoadData[x, y, z - 1] == 0) HandleMeshFace(Dir.Nz, HandleMeshAutoTile(x, y, z, Dir.Nz)); // Back
                        }
                    }
                }
 
            }
        }

        void HandleMeshFace(Dir direction, int textureIndex)
        {
            Count[_blockPosition.y]++;
            int vertexIndex = Vertices.Length; 
            _normal = Vector3.zero;

            if (direction == Dir.Py) // top
            {
                FaceVertices[0] = _blockPosition + new Vector3(0, 1, 0);
                FaceVertices[1] = _blockPosition + new Vector3(1, 1, 0);
                FaceVertices[2] = _blockPosition + new Vector3(1, 1, 1);
                FaceVertices[3] = _blockPosition + new Vector3(0, 1, 1);

                FaceVerticesShadow[0] = _blockPosition + new Vector3(-0.03f, 1, -0.03f);
                FaceVerticesShadow[1] = _blockPosition + new Vector3(1.03f, 1, -0.03f);
                FaceVerticesShadow[2] = _blockPosition + new Vector3(1.03f, 1, 1.03f);
                FaceVerticesShadow[3] = _blockPosition + new Vector3(-0.03f, 1, 1.03f);
                 
                Triangles.Add(vertexIndex);
                Triangles.Add(vertexIndex + 2);
                Triangles.Add(vertexIndex + 1);
                Triangles.Add(vertexIndex);
                Triangles.Add(vertexIndex + 3);
                Triangles.Add(vertexIndex + 2);
                _normal = new Vector3(0, 1, 0);
            } else if (direction == Dir.Ny) // down
            {
                FaceVertices[0] = _blockPosition + new Vector3(0, 0, 1);
                FaceVertices[1] = _blockPosition + new Vector3(1, 0, 1);
                FaceVertices[2] = _blockPosition + new Vector3(1, 0, 0);
                FaceVertices[3] = _blockPosition + new Vector3(0, 0, 0);
                Triangles.Add(vertexIndex);
                Triangles.Add(vertexIndex + 2);
                Triangles.Add(vertexIndex + 1);
                Triangles.Add(vertexIndex);
                Triangles.Add(vertexIndex + 3);
                Triangles.Add(vertexIndex + 2);
                _normal = new Vector3(0, -1, 0);
            }
            else if (direction == Dir.Pz) // back
            {
                FaceVertices[0] = _blockPosition + new Vector3(0, 0, 1);
                FaceVertices[1] = _blockPosition + new Vector3(1, 0, 1);
                FaceVertices[2] = _blockPosition + new Vector3(1, 1, 1);
                FaceVertices[3] = _blockPosition + new Vector3(0, 1, 1);
                Triangles.Add(vertexIndex);
                Triangles.Add(vertexIndex + 1);
                Triangles.Add(vertexIndex + 2);
                Triangles.Add(vertexIndex);
                Triangles.Add(vertexIndex + 2);
                Triangles.Add(vertexIndex + 3);
                _normal = new Vector3(0, 0, 1);
            }
            else if (direction == Dir.Nz) // front
            {
                FaceVertices[0] = _blockPosition + new Vector3(0, 0, 0);
                FaceVertices[1] = _blockPosition + new Vector3(1, 0, 0);
                FaceVertices[2] = _blockPosition + new Vector3(1, 1, 0);
                FaceVertices[3] = _blockPosition + new Vector3(0, 1, 0);
                Triangles.Add(vertexIndex);
                Triangles.Add(vertexIndex + 2);
                Triangles.Add(vertexIndex + 1);
                Triangles.Add(vertexIndex);
                Triangles.Add(vertexIndex + 3);
                Triangles.Add(vertexIndex + 2);
                _normal = new Vector3(0, 0, -1);
            }
            else if (direction == Dir.Nx) // left
            {
                FaceVertices[0] = _blockPosition + new Vector3(0, 0, 0);
                FaceVertices[1] = _blockPosition + new Vector3(0, 0, 1);
                FaceVertices[2] = _blockPosition + new Vector3(0, 1, 1);
                FaceVertices[3] = _blockPosition + new Vector3(0, 1, 0);
                Triangles.Add(vertexIndex);
                Triangles.Add(vertexIndex + 1);
                Triangles.Add(vertexIndex + 2);
                Triangles.Add(vertexIndex);
                Triangles.Add(vertexIndex + 2);
                Triangles.Add(vertexIndex + 3);
                _normal = new Vector3(-1, 0, 0);
            }
            else if (direction == Dir.Px) // right
            {
                FaceVertices[0] = _blockPosition + new Vector3(1, 0, 0);
                FaceVertices[1] = _blockPosition + new Vector3(1, 0, 1);
                FaceVertices[2] = _blockPosition + new Vector3(1, 1, 1);
                FaceVertices[3] = _blockPosition + new Vector3(1, 1, 0);
                Triangles.Add(vertexIndex);
                Triangles.Add(vertexIndex + 2);
                Triangles.Add(vertexIndex + 1);
                Triangles.Add(vertexIndex);
                Triangles.Add(vertexIndex + 3);
                Triangles.Add(vertexIndex + 2);
                _normal = new Vector3(1, 0, 0);
            }
 
 
            Vector2Int tile = GetTileRect(textureIndex);
            Vector2[] _spriteUVs = new Vector2[]
            {
                new Vector2(tile.x, tile.y),
                new Vector2(tile.x + TileSize, tile.y),
                new Vector2(tile.x + TileSize, tile.y + TileSize),
                new Vector2(tile.x, tile.y + TileSize)
            };

            _textureRect = TextureRectDictionary[_blockID];

            // Calculate the new UVs based on the original rect's position and size
            for (int i = 0; i < _spriteUVs.Length; i++)
            {
                _spriteUVs[i] = new Vector2(
                    (_spriteUVs[i].x / TextureAtlasWidth) + _textureRect.x,
                    (_spriteUVs[i].y / TextureAtlasHeight) + _textureRect.y
                );
            }
 
            // Add each UV coordinate individually
            for (int i = 0; i < _spriteUVs.Length; i++)
            {
                Uvs.Add(_spriteUVs[i]);
                Normals.Add(_normal);
                Vertices.Add(FaceVertices[i]);

                if (direction == Dir.Py) 
                {
                    VerticesShadow.Add(FaceVerticesShadow[i]);
                }
                else
                {
                    VerticesShadow.Add(FaceVertices[i]);
                }
            } 
        }

        Vector2Int GetTileRect(int index)
        { 
            int targetRow = index / TilesPerRow;
            int targetCol = index % TilesPerRow;  

            return new Vector2Int(Colx[targetCol], Rowy[targetRow]);
        }
          

        int HandleMeshAutoTile(int x, int y, int z, Dir mode)
        {
            _spriteNumber = 0;
            _edgeValue = 0;
            _cornerValue = 0;

            if (mode == Dir.Py) // Top
            {
                bool isPy = MapLoadData[x, y + 1, z] == 0;

                if ((MapLoadData[x, y, z + 1] != 0)
                    || (isPy && MapLoadData[x, y + 1, z + 1] != 0))
                {
                    _edgeValue += 1; // Top
                }

                if ((MapLoadData[x + 1, y, z] != 0)
                    || (isPy && MapLoadData[x + 1, y + 1, z] != 0))
                {
                    _edgeValue += 2; // Right
                }

                if ((MapLoadData[x, y, z - 1] != 0)
                    || (isPy && MapLoadData[x, y + 1, z - 1] != 0))
                {
                    _edgeValue += 4; // Bottom
                }

                if ((MapLoadData[x - 1, y, z] != 0)
                    || (isPy && MapLoadData[x - 1, y + 1, z] != 0))
                {
                    _edgeValue += 8; // Left
                }

                // Calculate corner values
                if (((MapLoadData[x - 1, y, z + 1] != 0)
                    || (isPy && MapLoadData[x - 1, y + 1, z + 1] != 0))
                    && (_edgeValue & 1) != 0 && (_edgeValue & 8) != 0)
                {
                    _cornerValue += 1; // Top-Left
                }

                if (((MapLoadData[x + 1, y, z + 1] != 0)
                    || (isPy && MapLoadData[x + 1, y + 1, z + 1] != 0))
                    && (_edgeValue & 1) != 0 && (_edgeValue & 2) != 0)
                {
                    _cornerValue += 2; // Top-Right
                }

                if (((MapLoadData[x + 1, y, z - 1] != 0)
                    || (isPy && MapLoadData[x + 1, y + 1, z - 1] != 0))
                    && (_edgeValue & 2) != 0 && (_edgeValue & 4) != 0)
                {
                    _cornerValue += 4; // Bottom-Right
                }

                if (((MapLoadData[x - 1, y, z - 1] != 0)
                    || (isPy && MapLoadData[x - 1, y + 1, z - 1] != 0))
                    && (_edgeValue & 4) != 0 && (_edgeValue & 8) != 0)
                {
                    _cornerValue += 8; // Bottom-Left
                }
            }  
            else if (mode == Dir.Nx) // Negative X (Left side of the cube)
            {
                bool isNx = MapLoadData[x - 1, y, z] == 0;

                if ((MapLoadData[x, y + 1, z] != 0)
                    || (isNx && MapLoadData[x - 1, y + 1, z] != 0))
                {
                    _edgeValue += 1; // Top
                }

                if ((MapLoadData[x, y, z + 1] != 0)
                    || (isNx && MapLoadData[x - 1, y, z + 1] != 0))
                {
                    _edgeValue += 2; // Right
                }

                if ((MapLoadData[x, y - 1, z] != 0)
                    || (isNx && MapLoadData[x - 1, y - 1, z] != 0))
                {
                    _edgeValue += 4; // Bottom
                }

                if ((MapLoadData[x, y, z - 1] != 0)
                    || (isNx && MapLoadData[x - 1, y, z - 1] != 0))
                {
                    _edgeValue += 8; // Left
                }

                // Calculate corner values
                
                if (((MapLoadData[x, y + 1, z - 1] != 0)
                     || (isNx && MapLoadData[x - 1, y + 1, z - 1] != 0))
                    && (_edgeValue & 1) != 0 && (_edgeValue & 8) != 0)
                {
                    _cornerValue += 1; // Top-Left
                }
                if (((MapLoadData[x, y + 1, z + 1] != 0)
                     || (isNx && MapLoadData[x - 1, y + 1, z + 1] != 0))
                    && (_edgeValue & 1) != 0 && (_edgeValue & 2) != 0)
                {
                    _cornerValue += 2; // Top-Right
                }

                if (((MapLoadData[x, y - 1, z + 1] != 0)
                     || (isNx && MapLoadData[x - 1, y - 1, z + 1] != 0))
                    && (_edgeValue & 2) != 0 && (_edgeValue & 4) != 0)
                {
                    _cornerValue += 4; // Bottom-Right
                }

                if (((MapLoadData[x, y - 1, z - 1] != 0)
                     || (isNx && MapLoadData[x - 1, y - 1, z - 1] != 0))
                    && (_edgeValue & 4) != 0 && (_edgeValue & 8) != 0)
                {
                    _cornerValue += 8; // Bottom-Left
                } 
            }
            else if (mode == Dir.Px) // Positive X (Right side of the cube)
            {
                bool isPx = MapLoadData[x + 1, y, z] == 0;

                if ((MapLoadData[x, y + 1, z] != 0)
                    || (isPx && MapLoadData[x + 1, y + 1, z] != 0))
                {
                    _edgeValue += 1; // Top
                }

                if ((MapLoadData[x, y, z + 1] != 0)
                    || (isPx && MapLoadData[x + 1, y, z + 1] != 0))
                {
                    _edgeValue += 2; // Right
                }

                if ((MapLoadData[x, y - 1, z] != 0)
                    || (isPx && MapLoadData[x + 1, y - 1, z] != 0))
                {
                    _edgeValue += 4; // Bottom
                }

                if ((MapLoadData[x, y, z - 1] != 0)
                    || (isPx && MapLoadData[x + 1, y, z - 1] != 0))
                {
                    _edgeValue += 8; // Left
                }

                // Lib.Log(x,y,z,_chunkMap[x, y + 1, z] != 0, _chunkMap[x, y, z + 1] != 0,_chunkMap[x, y - 1, z] != 0, _chunkMap[x, y, z - 1] != 0, _chunkMap[x, y + 1, z + 1] != 0, _chunkMap[x, y - 1, z + 1] != 0, _chunkMap[x, y - 1, z - 1] != 0, _chunkMap[x, y + 1, z - 1] != 0);
                // Calculate corner values

                if (((MapLoadData[x, y + 1, z - 1] != 0)
                     || (isPx && MapLoadData[x + 1, y + 1, z - 1] != 0))
                    && (_edgeValue & 1) != 0 && (_edgeValue & 8) != 0)
                {
                    _cornerValue += 1; // Top-Left
                }
                
                if (((MapLoadData[x, y + 1, z + 1] != 0)
                     || (isPx && MapLoadData[x + 1, y + 1, z + 1] != 0))
                    && (_edgeValue & 1) != 0 && (_edgeValue & 2) != 0)
                {
                    _cornerValue += 2; // Top-Right
                }

                if (((MapLoadData[x, y - 1, z + 1] != 0)
                     || (isPx && MapLoadData[x + 1, y - 1, z + 1] != 0))
                    && (_edgeValue & 2) != 0 && (_edgeValue & 4) != 0)
                {
                    _cornerValue += 4; // Bottom-Right
                }

                if (((MapLoadData[x, y - 1, z - 1] != 0)
                     || (isPx && MapLoadData[x + 1, y - 1, z - 1] != 0))
                    && (_edgeValue & 4) != 0 && (_edgeValue & 8) != 0)
                {
                    _cornerValue += 8; // Bottom-Left
                } 
            }
            else if (mode == Dir.Pz) // Positive Z (Front of the cube)
            {
                bool isPz = MapLoadData[x, y, z + 1] == 0;

                if ((MapLoadData[x, y + 1, z] != 0)
                    || (isPz && MapLoadData[x, y + 1, z + 1] != 0))
                {
                    _edgeValue += 1; // Top
                }

                if ((MapLoadData[x + 1, y, z] != 0)
                    || (isPz && MapLoadData[x + 1, y, z + 1] != 0))
                {
                    _edgeValue += 2; // Right
                }

                if ((MapLoadData[x, y - 1, z] != 0)
                    || (isPz && MapLoadData[x, y - 1, z + 1] != 0))
                {
                    _edgeValue += 4; // Bottom
                }

                if ((MapLoadData[x - 1, y, z] != 0)
                    || (isPz && MapLoadData[x - 1, y, z + 1] != 0))
                {
                    _edgeValue += 8; // Left
                }

                // Calculate corner values
                if (((MapLoadData[x - 1, y + 1, z] != 0)
                     || (isPz && MapLoadData[x - 1, y + 1, z + 1] != 0))
                    && (_edgeValue & 1) != 0 && (_edgeValue & 8) != 0)
                {
                    _cornerValue += 1; // Top-Left
                }

                if (((MapLoadData[x + 1, y + 1, z] != 0)
                     || (isPz && MapLoadData[x + 1, y + 1, z + 1] != 0))
                    && (_edgeValue & 1) != 0 && (_edgeValue & 2) != 0)
                {
                    _cornerValue += 2; // Top-Right
                }

                if (((MapLoadData[x + 1, y - 1, z] != 0)
                     || (isPz && MapLoadData[x + 1, y - 1, z + 1] != 0))
                    && (_edgeValue & 2) != 0 && (_edgeValue & 4) != 0)
                {
                    _cornerValue += 4; // Bottom-Right
                }

                if (((MapLoadData[x - 1, y - 1, z] != 0)
                     || (isPz && MapLoadData[x - 1, y - 1, z + 1] != 0))
                    && (_edgeValue & 4) != 0 && (_edgeValue & 8) != 0)
                {
                    _cornerValue += 8; // Bottom-Left
                }
            }
            else if (mode == Dir.Nz) // Negative Z (Back of the cube)
            {
                bool isNz = MapLoadData[x, y, z - 1] == 0;

                if ((MapLoadData[x, y + 1, z] != 0)
                    || (isNz && MapLoadData[x, y + 1, z - 1] != 0))
                {
                    _edgeValue += 1; // Top
                }

                if ((MapLoadData[x + 1, y, z] != 0)
                    || (isNz && MapLoadData[x + 1, y, z - 1] != 0))
                {
                    _edgeValue += 2; // Right
                }

                if ((MapLoadData[x, y - 1, z] != 0)
                    || (isNz && MapLoadData[x, y - 1, z - 1] != 0))
                {
                    _edgeValue += 4; // Bottom
                }

                if ((MapLoadData[x - 1, y, z] != 0)
                    || (isNz && MapLoadData[x - 1, y, z - 1] != 0))
                {
                    _edgeValue += 8; // Left
                }

                // Calculate corner values
                if (((MapLoadData[x - 1, y + 1, z] != 0)
                     || (isNz && MapLoadData[x - 1, y + 1, z - 1] != 0))
                    && (_edgeValue & 1) != 0 && (_edgeValue & 8) != 0)
                {
                    _cornerValue += 1; // Top-Left
                }

                if (((MapLoadData[x + 1, y + 1, z] != 0)
                     || (isNz && MapLoadData[x + 1, y + 1, z - 1] != 0))
                    && (_edgeValue & 1) != 0 && (_edgeValue & 2) != 0)
                {
                    _cornerValue += 2; // Top-Right
                }

                if (((MapLoadData[x + 1, y - 1, z] != 0)
                     || (isNz && MapLoadData[x + 1, y - 1, z - 1] != 0))
                    && (_edgeValue & 2) != 0 && (_edgeValue & 4) != 0)
                {
                    _cornerValue += 4; // Bottom-Right
                }

                if (((MapLoadData[x - 1, y - 1, z] != 0)
                     || (isNz && MapLoadData[x - 1, y - 1, z - 1] != 0))
                    && (_edgeValue & 4) != 0 && (_edgeValue & 8) != 0)
                {
                    _cornerValue += 8; // Bottom-Left
                }
            } 





         

            // Determine the tile number using nested switch statements
            switch (_edgeValue)
            {
                case 0: _spriteNumber = 36; break;
                case 1: _spriteNumber = 24; break;
                case 2: _spriteNumber = 37; break;
                case 3:
                    switch (_cornerValue)
                    {
                        case 0: _spriteNumber = 25; break;
                        case 2: _spriteNumber = 44; break;
                    }
                    break;
                case 4: _spriteNumber = 0; break;
                case 5: _spriteNumber = 12; break;
                case 6:
                    switch (_cornerValue)
                    {
                        case 0: _spriteNumber = 1; break;
                        case 4: _spriteNumber = 8; break;
                    }
                    break;
                case 7:
                    switch (_cornerValue)
                    {
                        case 0: _spriteNumber = 13; break;
                        case 2: _spriteNumber = 28; break;
                        case 4: _spriteNumber = 16; break;
                        case 6: _spriteNumber = 20; break;
                    }
                    break;
                case 8: _spriteNumber = 39; break;
                case 9:
                    switch (_cornerValue)
                    {
                        case 0: _spriteNumber = 27; break;
                        case 1: _spriteNumber = 47; break;
                    }
                    break;
                case 10: _spriteNumber = 38; break;
                case 11:
                    switch (_cornerValue)
                    {
                        case 0: _spriteNumber = 26; break;
                        case 1: _spriteNumber = 42; break;
                        case 2: _spriteNumber = 41; break;
                        case 3: _spriteNumber = 45; break;
                    }
                    break;
                case 12:
                    switch (_cornerValue)
                    {
                        case 0: _spriteNumber = 3; break;
                        case 8: _spriteNumber = 11; break;
                    }
                    break;
                case 13:
                    switch (_cornerValue)
                    {
                        case 0: _spriteNumber = 15; break;
                        case 1: _spriteNumber = 31; break;
                        case 8: _spriteNumber = 19; break;
                        case 9: _spriteNumber = 35; break;
                    }
                    break;
                case 14:
                    switch (_cornerValue)
                    {
                        case 0: _spriteNumber = 2; break;
                        case 4: _spriteNumber = 5; break;
                        case 8: _spriteNumber = 6; break;
                        case 12: _spriteNumber = 10; break;
                    }
                    break;
                case 15:
                    switch (_cornerValue)
                    {
                        case 0: _spriteNumber = 14; break;
                        case 1: _spriteNumber = 4; break;
                        case 2: _spriteNumber = 7; break;
                        case 3: _spriteNumber = 46; break;
                        case 4: _spriteNumber = 43; break;
                        case 5: _spriteNumber = 34; break;
                        case 6: _spriteNumber = 32; break;
                        case 7: _spriteNumber = 29; break;
                        case 8: _spriteNumber = 40; break;
                        case 9: _spriteNumber = 23; break;
                        case 10: _spriteNumber = 21; break;
                        case 11: _spriteNumber = 30; break;
                        case 12: _spriteNumber = 9; break;
                        case 13: _spriteNumber = 18; break;
                        case 14: _spriteNumber = 17; break;
                        case 15: _spriteNumber = 33; break;
                    }
                    break;
            }
            
            if (mode != Dir.Py){
                _spriteNumber += 48;
            }
            return _spriteNumber;
        }
 
    }
}



 
 