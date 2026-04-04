using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
// i fucking hate this script
public class MapCullComponent : MonoBehaviour
{  
    public Mesh _meshData;
    public ChunkMeshNativeData _nativeMeshData;

    private MeshCollider _meshCollider;
    private MeshFilter _shadowMeshFilter;
    private MeshFilter _meshFilter; 
    private MeshRenderer _meshRenderer;
    private Mesh _mesh;  
    private NativeList<Vector3> _culledVerticesNative;
    private NativeList<Vector3> _culledNormalsNative;
    private NativeList<int> _culledTrianglesNative;
    private NativeList<Vector2> _culledUVsNative;
    private readonly List<int> _shadowTrianglesManagedCache = new List<int>();
    private readonly List<int> _culledTrianglesManagedCache = new List<int>();
    
    private int CULL_DISTANCE = 2; 
    private Vector3Int _selfChunkPosition;
    private Vector3Int playerChunkPosition;  
 
    void Awake()
    {   
        CULL_DISTANCE= World.ChunkSize * CULL_DISTANCE;
        _mesh = new Mesh();    
        EnsureCullOutputBuffers();

        _meshFilter = GetComponent<MeshFilter>();  
        _meshRenderer = GetComponent<MeshRenderer>(); 
        _meshCollider = gameObject.AddComponent<MeshCollider>();
        _meshCollider.includeLayers = Main.MaskMap;
        _meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _meshRenderer.enabled = false;    
        _selfChunkPosition = Vector3Int.FloorToInt(transform.position);
    }

    public void SetMeshData(Mesh meshData, ChunkMeshNativeData nativeMeshData)
    {
        if (_nativeMeshData != null && _nativeMeshData != nativeMeshData)
        {
            _nativeMeshData.Dispose();
        }

        _meshData = meshData;
        _nativeMeshData = nativeMeshData;
    }

    void Start() { 
        CreateShadowMesh();
        HandleAssignment();    
        EntityStaticLoad.LoadEntitiesInChunk(_selfChunkPosition);
    }  
    void CreateShadowMesh()
    {  
        GameObject shadowObject = new GameObject("Shadow");
        shadowObject.transform.parent = transform;
        shadowObject.transform.position = transform.position;

        _shadowMeshFilter = shadowObject.AddComponent<MeshFilter>();
        MeshRenderer shadowMeshRenderer = shadowObject.AddComponent<MeshRenderer>();
        shadowMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        shadowMeshRenderer.material = Block.ShadowMeshMaterial; 
    }
   
    public void HandleAssignment()
    {
        try
        {
            if (_meshData == null || _nativeMeshData == null || !_nativeMeshData.IsCreated)
            {
                return;
            }

            Mesh shadowMesh = _shadowMeshFilter.sharedMesh;
            if (shadowMesh == null)
            {
                shadowMesh = new Mesh();
            }
            else
            {
                shadowMesh.Clear();
            }

            shadowMesh.SetVertices(_nativeMeshData.VerticesShadow);
            CopyNativeArrayToList(_nativeMeshData.Triangles, _shadowTrianglesManagedCache);
            shadowMesh.SetTriangles(_shadowTrianglesManagedCache, 0);
            shadowMesh.SetNormals(_nativeMeshData.Normals);
            _shadowMeshFilter.sharedMesh = shadowMesh;

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
            if (MapCull.YCheck)
            { 
                if (_selfChunkPosition.y + World.ChunkSize < MapCull.YThreshold)  // lower chunks
                {
                    while (Time.frameCount < MapCull.CullSyncFrame + 2) await Task.Yield();
                    _meshRenderer.enabled = true;
                    _meshFilter.mesh = _meshData;
                    return; 
                }           
                if (_selfChunkPosition.y >= MapCull.YThreshold) // higher chunks (invis)
                {
                    while (Time.frameCount < MapCull.CullSyncFrame) await Task.Yield();
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

        _mesh.SetVertices(_culledVerticesNative.AsArray());
        CopyNativeArrayToList(_culledTrianglesNative.AsArray(), _culledTrianglesManagedCache);
        _mesh.SetTriangles(_culledTrianglesManagedCache, 0);
        _mesh.SetUVs(0, _culledUVsNative.AsArray());
        _mesh.SetNormals(_culledNormalsNative.AsArray());
         
        while (Time.frameCount < MapCull.CullSyncFrame) await Task.Yield();
        
        if (MapCull.YCheck) _meshFilter.mesh = _mesh; 
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
            playerChunkPosition = Scene.PlayerChunkPosition;
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
        if (_nativeMeshData == null || !_nativeMeshData.IsCreated)
        {
            return;
        }

        EnsureCullOutputBuffers();
        _culledVerticesNative.Clear();
        _culledNormalsNative.Clear();
        _culledTrianglesNative.Clear();
        _culledUVsNative.Clear();

        var job = new HandleCullMathJob
        { 
            chunkSize = World.ChunkSize, 
            yThreshold = MapCull.YThreshold - _selfChunkPosition.y,

            meshLoadData = MeshLoadData.Create(_selfChunkPosition),
            vertices = _nativeMeshData.Vertices,
            normals = _nativeMeshData.Normals,
            triangles = _nativeMeshData.Triangles,
            uvs = _nativeMeshData.Uvs,
            count = _nativeMeshData.Count,

            culledVertices = _culledVerticesNative,
            culledNormals = _culledNormalsNative,
            culledTriangles = _culledTrianglesNative,
            culledUVs = _culledUVsNative,

            vertexMap = new NativeHashMap<int, int>(_nativeMeshData.Vertices.Length, Allocator.TempJob), 
        };

        JobHandle handle = job.Schedule();
        handle.Complete();

        job.vertexMap.Dispose();
    }
 
    private static void CopyNativeArrayToList(NativeArray<int> source, List<int> destination)
    {
        destination.Clear();
        int len = source.Length;
        if (destination.Capacity < len)
        {
            destination.Capacity = len;
        }

        for (int i = 0; i < len; i++)
        {
            destination.Add(source[i]);
        }
    }

    private void DisposeNativeBuffers()
    {
        if (_nativeMeshData != null)
        {
            _nativeMeshData.Dispose();
            _nativeMeshData = null;
        }
        if (_culledVerticesNative.IsCreated) _culledVerticesNative.Dispose();
        if (_culledNormalsNative.IsCreated) _culledNormalsNative.Dispose();
        if (_culledTrianglesNative.IsCreated) _culledTrianglesNative.Dispose();
        if (_culledUVsNative.IsCreated) _culledUVsNative.Dispose();
    }

    private void EnsureCullOutputBuffers()
    {
        if (!_culledVerticesNative.IsCreated) _culledVerticesNative = new NativeList<Vector3>(Allocator.Persistent);
        if (!_culledNormalsNative.IsCreated) _culledNormalsNative = new NativeList<Vector3>(Allocator.Persistent);
        if (!_culledTrianglesNative.IsCreated) _culledTrianglesNative = new NativeList<int>(Allocator.Persistent);
        if (!_culledUVsNative.IsCreated) _culledUVsNative = new NativeList<Vector2>(Allocator.Persistent);
    }

    private void OnDestroy()
    {
        DisposeNativeBuffers();
    }


 
    public struct HandleCullMathJob : IJob
    {  
        [DeallocateOnJobCompletion]
        public int yThreshold;
        [DeallocateOnJobCompletion]
        public int chunkSize; 

        [DeallocateOnJobCompletion]
        public NativeMap3D<int> meshLoadData;
        public NativeArray<Vector3> vertices;
        public NativeArray<Vector3> normals;
        public NativeArray<int> triangles;
        public NativeArray<Vector2> uvs;  
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
                    if (meshLoadData[x, yThreshold, z] != 0 // have block on top of the face burying it
                        && meshLoadData[x, yThreshold - 1, z] != 0) // block the face belongs to exists
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
            bool isPy = meshLoadData[x, y + 1, z] == 0;

            if ((meshLoadData[x, y, z + 1] != 0)
                || (isPy && meshLoadData[x, y + 1, z + 1] != 0))
            {
                edgeValue += 1; // Top
            }

            if ((meshLoadData[x + 1, y, z] != 0)
                || (isPy && meshLoadData[x + 1, y + 1, z] != 0))
            {
                edgeValue += 2; // Right
            }

            if ((meshLoadData[x, y, z - 1] != 0)
                || (isPy && meshLoadData[x, y + 1, z - 1] != 0))
            {
                edgeValue += 4; // Bottom
            }

            if ((meshLoadData[x - 1, y, z] != 0)
                || (isPy && meshLoadData[x - 1, y + 1, z] != 0))
            {
                edgeValue += 8; // Left
            }

            // Calculate corner values
            if (((meshLoadData[x - 1, y, z + 1] != 0)
                || (isPy && meshLoadData[x - 1, y + 1, z + 1] != 0))
                && (edgeValue & 1) != 0 && (edgeValue & 8) != 0)
            {
                cornerValue += 1; // Top-Left
            }

            if (((meshLoadData[x + 1, y, z + 1] != 0)
                || (isPy && meshLoadData[x + 1, y + 1, z + 1] != 0))
                && (edgeValue & 1) != 0 && (edgeValue & 2) != 0)
            {
                cornerValue += 2; // Top-Right
            }

            if (((meshLoadData[x + 1, y, z - 1] != 0)
                || (isPy && meshLoadData[x + 1, y + 1, z - 1] != 0))
                && (edgeValue & 2) != 0 && (edgeValue & 4) != 0)
            {
                cornerValue += 4; // Bottom-Right
            }

            if (((meshLoadData[x - 1, y, z - 1] != 0)
                || (isPy && meshLoadData[x - 1, y + 1, z - 1] != 0))
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