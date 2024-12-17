using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Profiling;
// i fucking hate this script
public class MapCullModule : MonoBehaviour
{  
    public Mesh _meshData;
    public List<Vector3> _verticesShadow;
    public int[] _count;

    private MeshCollider _meshCollider;
    private MeshFilter _shadowMeshFilter;
    private MeshFilter _meshFilter; 
    private MeshRenderer _meshRenderer;
    private Mesh _mesh;  
    
    private int CULL_DISTANCE = 2; 
    private Vector3Int _selfChunkPosition;
    private Vector3Int playerChunkPosition;  
 
    void Awake()
    {   
        CULL_DISTANCE= World.ChunkSize * CULL_DISTANCE;
        _mesh = new Mesh();    

        _meshFilter = GetComponent<MeshFilter>();  
        _meshRenderer = GetComponent<MeshRenderer>(); 
        _meshCollider = gameObject.AddComponent<MeshCollider>();
        _meshCollider.includeLayers = Game.MaskMap;
        _meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _meshRenderer.enabled = false;    
        _selfChunkPosition = Vector3Int.FloorToInt(transform.position);
    }

    void Start() { 
        CreateShadowMesh();
        HandleAssignment();    
        EntityStaticLoadSingleton.Instance.LoadEntitiesInChunk(_selfChunkPosition);
    }  
    void CreateShadowMesh()
    {  
        GameObject shadowObject = new GameObject("Shadow"); 
        shadowObject.transform.parent = transform;
        shadowObject.transform.position = transform.position;

        _shadowMeshFilter = shadowObject.AddComponent<MeshFilter>();
        MeshRenderer shadowMeshRenderer = shadowObject.AddComponent<MeshRenderer>();
        shadowMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        shadowMeshRenderer.material = BlockSingleton.ShadowMeshMaterial; 
    }
   
    public void HandleAssignment()
    {
        try
        {
            _uvs = _meshData.uv;
            _vertices = _meshData.vertices;
            _normals = _meshData.normals;
            _triangles = _meshData.triangles;   

            Mesh shadowMesh = new Mesh();
            shadowMesh.SetVertices(_verticesShadow);
            shadowMesh.SetTriangles(_meshData.triangles, 0);   
            shadowMesh.SetNormals(_meshData.normals); 
            _shadowMeshFilter.mesh = shadowMesh;

            CullMeshAsync();     
            _meshCollider.sharedMesh = _meshData;
            _meshCollider.convex = false; 
            _meshCollider.isTrigger = false;  
        }
        catch (Exception ex)
        { 
            if (ex is not MissingReferenceException && ex is not NullReferenceException)
            {
                throw new Exception("An exception occurred in GetPath method.", ex);
            }
        }
    }

    private SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
    public async void CullMeshAsync()
    { 
        await _semaphoreSlim.WaitAsync();
        try {
            if (MapCullSingleton.Instance._yCheck)
            { 
                if (_selfChunkPosition.y + World.ChunkSize < MapCullSingleton.Instance._yThreshold)  // lower chunks
                {
                    while (Time.frameCount < MapCullSingleton.Instance._cullSyncFrame + 2) await Task.Yield();
                    _meshRenderer.enabled = true;
                    _meshFilter.mesh = _meshData;
                    return; 
                }           
                if (_selfChunkPosition.y >= MapCullSingleton.Instance._yThreshold) // higher chunks (invis)
                {
                    while (Time.frameCount < MapCullSingleton.Instance._cullSyncFrame) await Task.Yield();
                    _meshRenderer.enabled = false;
                    // _meshFilter.mesh = _meshData;
                    return;
                }
                if (HandleRangeCheck()) // do cull, is in range
                { 
                    await Task.Run(() => HandleCullMath());
                    HandleCullMesh();
                }
                // else
                // {
                //     if ((int)Game.Player.transform.position.y < HIDE_ALL_Y) // cull but out of range and player is low ???? 
                //     {
                //         Lib.Log();
                //         _meshRenderer.enabled = false;
                //         _meshFilter.mesh = _meshData;
                //     }
                // } 
            }
            else // no cull, revert to normal
            {
                _meshRenderer.enabled = true;
                _meshFilter.mesh = _meshData;
            }
        } catch (Exception ex) {
            if (ex is not MissingReferenceException && ex is not NullReferenceException)
                throw new Exception("An exception occurred in CullMeshAsync method.", ex);
        } finally {
            _semaphoreSlim.Release(); // called even if return
        }
    }



 
    async void HandleCullMesh()
    {   
        _mesh = new Mesh();
        _mesh.SetVertices(_culledVertices);
        _mesh.SetTriangles(_culledTriangles, 0);  
        _mesh.SetUVs(0, _culledUVs);
        _mesh.SetNormals(_culledNormals);     
         
        while (Time.frameCount < MapCullSingleton.Instance._cullSyncFrame) await Task.Yield();
        
        if (MapCullSingleton.Instance._yCheck) _meshFilter.mesh = _mesh; 
        await Task.Yield();   //do not remove
        await Task.Yield();   //do not remove
        _meshRenderer.enabled = true;
    } 
      
    float xDist;
    float zDist;
    bool xCheck;
    bool zCheck; 
    bool HandleRangeCheck()
    {
        // return true;
        try
        { 
            //TODO y
            playerChunkPosition = WorldSingleton.PlayerChunkPosition;
            xDist = Mathf.Abs(playerChunkPosition.x - transform.position.x);
            zDist = Mathf.Abs(playerChunkPosition.z - transform.position.z);   
            xCheck = xDist <= CULL_DISTANCE; 
            zCheck = zDist <= CULL_DISTANCE;
            return xCheck && zCheck; 
        }
        catch 
        {  
            return false; 
        }
    }  
    void HandleCullMath()
    {
        NativeArray<Vector3> vertices = new NativeArray<Vector3>(_vertices, Allocator.TempJob);
        NativeArray<Vector3> normals = new NativeArray<Vector3>(_normals, Allocator.TempJob);
        NativeArray<int> triangles = new NativeArray<int>(_triangles, Allocator.TempJob);
        NativeArray<Vector2> uvs = new NativeArray<Vector2>(_uvs, Allocator.TempJob);
        NativeArray<int> count = new NativeArray<int>(_count, Allocator.TempJob);
        var job = new HandleCullMathJob
        { 
            chunkSize = World.ChunkSize, 
            yThreshold = MapCullSingleton.Instance._yThreshold - _selfChunkPosition.y,

            mapLoadData = MapLoadData.Create(_selfChunkPosition),
            vertices = vertices,
            normals = normals,
            triangles = triangles,
            uvs = uvs, 
            count = count,

            culledVertices  = new NativeList<Vector3>(Allocator.TempJob),
            culledNormals  = new NativeList<Vector3>(Allocator.TempJob),
            culledTriangles = new NativeList<int>(Allocator.TempJob),
            culledUVs = new NativeList<Vector2>(Allocator.TempJob), 

            vertexMap = new NativeHashMap<int, int>(vertices.Length, Allocator.TempJob), 
        };

        JobHandle handle = job.Schedule();
        handle.Complete();

        // Use the results from the job
        _culledVertices = new List<Vector3>(job.culledVertices.ToArray());
        _culledNormals = new List<Vector3>(job.culledNormals.ToArray());
        _culledTriangles = new List<int>(job.culledTriangles.ToArray());
        _culledUVs = new List<Vector2>(job.culledUVs.ToArray()); 
        // Dispose of the native collections 
        job.culledVertices.Dispose();
        job.culledNormals.Dispose();
        job.culledTriangles.Dispose();
        job.culledUVs.Dispose(); 

        job.vertexMap.Dispose(); 
    }



    private List<Vector3> _culledVertices;
    private List<Vector3> _culledNormals;
    private List<int> _culledTriangles;
    private List<Vector2> _culledUVs;
 
    private Vector3[] _vertices;
    private Vector3[] _normals; 
    private int[] _triangles;
    private Vector2[] _uvs;


 
    public struct HandleCullMathJob : IJob
    {  
        [DeallocateOnJobCompletion]
        public int yThreshold;
        [DeallocateOnJobCompletion]
        public int chunkSize; 

        [DeallocateOnJobCompletion]
        public NativeMap3D<int> mapLoadData;
        [DeallocateOnJobCompletion]
        public NativeArray<Vector3> vertices;
        [DeallocateOnJobCompletion]
        public NativeArray<Vector3> normals;
        [DeallocateOnJobCompletion]
        public NativeArray<int> triangles;
        [DeallocateOnJobCompletion]
        public NativeArray<Vector2> uvs;  
        [DeallocateOnJobCompletion]
        public NativeArray<int> count; 

        public NativeList<Vector3> culledVertices;
        public NativeList<Vector3> culledNormals;
        public NativeList<int> culledTriangles;
        public NativeList<Vector2> culledUVs;  

        public NativeHashMap<int, int> vertexMap;  

        public void Execute()
        {
            for (int i = 0; i < count[yThreshold-1]*4; i++)
            {
                vertexMap[i] = culledVertices.Length;
                culledVertices.Add(vertices[i]);
                culledNormals.Add(normals[i]);
                culledUVs.Add(uvs[i]);
            }

            for (int i = 0; i < count[yThreshold-1]*6; i += 3)
            {
                culledTriangles.Add(vertexMap[triangles[i]]);
                culledTriangles.Add(vertexMap[triangles[i + 1]]);
                culledTriangles.Add(vertexMap[triangles[i + 2]]);
            }
            
            for (int x = 0; x < chunkSize; x++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    if (mapLoadData[x, yThreshold, z] != 0 // have block on top of the face burying it
                        && mapLoadData[x, yThreshold - 1, z] != 0) // block the face belongs to exists
                    {
                        Vector3 vertex1 = new Vector3(x, yThreshold, z);
                        Vector3 vertex2 = new Vector3(x + 1, yThreshold, z);
                        Vector3 vertex3 = new Vector3(x, yThreshold, z + 1);
                        Vector3 vertex4 = new Vector3(x + 1, yThreshold, z + 1);

                        culledVertices.Add(vertex1);
                        culledVertices.Add(vertex2);
                        culledVertices.Add(vertex3);
                        culledVertices.Add(vertex4);

                        culledNormals.Add(Vector3.up);
                        culledNormals.Add(Vector3.up);
                        culledNormals.Add(Vector3.up);
                        culledNormals.Add(Vector3.up);

                        int index1 = culledVertices.Length - 4;
                        int index2 = culledVertices.Length - 3;
                        int index3 = culledVertices.Length - 2;
                        int index4 = culledVertices.Length - 1;

                        culledUVs.Add(new Vector2(0, 0));
                        culledUVs.Add(new Vector2(0, 0));
                        culledUVs.Add(new Vector2(0, 0));
                        culledUVs.Add(new Vector2(0, 0));

                        culledTriangles.Add(index1);
                        culledTriangles.Add(index3);
                        culledTriangles.Add(index2);
                        culledTriangles.Add(index2);
                        culledTriangles.Add(index3);
                        culledTriangles.Add(index4);
                    }
                }
            }
        }
        
        
        int HandleMeshAutoTile(int x, int y, int z)
        {
            int spriteNumber = 0;
            int edgeValue = 0;
            int cornerValue = 0;
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
            return spriteNumber;
        }
    }
} 