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
public class MapCullInst : MonoBehaviour
{  
    public int[,,] _chunkMap;
    public Mesh _meshData;
    public List<Vector3> _verticesShadow;

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
        CULL_DISTANCE= WorldStatic.CHUNK_SIZE * CULL_DISTANCE;
        _mesh = new Mesh();    

        _meshFilter = GetComponent<MeshFilter>();  
        _meshRenderer = GetComponent<MeshRenderer>(); 
        _meshCollider = gameObject.AddComponent<MeshCollider>();
        _meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _meshRenderer.enabled = false;    
        _selfChunkPosition = Vector3Int.FloorToInt(transform.position);
    }

    void Start() { 
        CreateShadowMesh();
        HandleAssignment();    
    }  
    void CreateShadowMesh()
    {  
        GameObject shadowObject = new GameObject("Shadow"); 
        shadowObject.transform.parent = transform;
        shadowObject.transform.position = transform.position;

        _shadowMeshFilter = shadowObject.AddComponent<MeshFilter>();
        MeshRenderer shadowMeshRenderer = shadowObject.AddComponent<MeshRenderer>();
        shadowMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        shadowMeshRenderer.material = BlockStatic.ShadowMeshMaterial; 
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
            if (MapCullStatic.Instance._yCheck)
            { 
                if (_selfChunkPosition.y + WorldStatic.CHUNK_SIZE < MapCullStatic.Instance._yThreshold)  // lower chunks
                {
                    _meshRenderer.enabled = true;
                    _meshFilter.mesh = _meshData;
                    return; 
                }           
                if (_selfChunkPosition.y >= MapCullStatic.Instance._yThreshold) // higher chunks (invis)
                {
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
         
        while (Time.frameCount < MapCullStatic.Instance._cullSyncFrame) await Task.Yield();
        
        if (MapCullStatic.Instance._yCheck) _meshFilter.mesh = _mesh; 
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
            playerChunkPosition = WorldStatic._playerChunkPos;
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
        var job = new HandleCullMathJob
        { 
            chunkSize = WorldStatic.CHUNK_SIZE, 
            yThreshold = MapCullStatic.Instance._yThreshold - _selfChunkPosition.y,

            chunkMap = ChunkMap.Create(_selfChunkPosition),
            vertices = vertices,
            normals = normals,
            triangles = triangles,
            uvs = uvs, 

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
        public NativeMap3D<int> chunkMap;
        [DeallocateOnJobCompletion]
        public NativeArray<Vector3> vertices;
        [DeallocateOnJobCompletion]
        public NativeArray<Vector3> normals;
        [DeallocateOnJobCompletion]
        public NativeArray<int> triangles;
        [DeallocateOnJobCompletion]
        public NativeArray<Vector2> uvs; 

        public NativeList<Vector3> culledVertices;
        public NativeList<Vector3> culledNormals;
        public NativeList<int> culledTriangles;
        public NativeList<Vector2> culledUVs; 

        public NativeHashMap<int, int> vertexMap;  

        public void Execute()
        { 

            for (int i = 0; i < vertices.Length; i++)
            {
                if (vertices[i].y <= yThreshold)
                {
                    vertexMap[i] = culledVertices.Length;
                    culledVertices.Add(vertices[i]);
                    culledNormals.Add(normals[i]);
                    culledUVs.Add(uvs[i]);
                }  
            }

            for (int i = 0; i < triangles.Length; i += 3)
            {
                if (vertexMap.ContainsKey(triangles[i]) && vertexMap.ContainsKey(triangles[i + 1]) && vertexMap.ContainsKey(triangles[i + 2]))
                {
                    culledTriangles.Add(vertexMap[triangles[i]]);
                    culledTriangles.Add(vertexMap[triangles[i + 1]]);
                    culledTriangles.Add(vertexMap[triangles[i + 2]]);
                } 
            }

            
            for (int x = 0; x < chunkSize; x++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    if (chunkMap[x, yThreshold, z] != 0 // have block on top of the face burying it
                        && chunkMap[x, yThreshold - 1, z] != 0) // block the face belongs to exists
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
    }

    public void Dispose()
    {
        array.Dispose();
    }
}