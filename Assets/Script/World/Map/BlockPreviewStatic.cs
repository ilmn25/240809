using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockPreviewStatic : MonoBehaviour
{ 
    public static BlockPreviewStatic Instance { get; private set; }  
    
    private float OPACITY = 0.45f;
    
    private GameObject meshObject;
    private List<Vector3> _vertices;
    private List<int> _triangles;
    private List<Vector2> _uvs;
    private List<Vector3> _normals;  
    
    void Start()
    {
        Instance = this;
    }

    public void DeleteBlock()
    {
        Destroy(meshObject);
    }

    public GameObject CreateBlock(string blockID)
    {
        GenerateMesh();
        
        Mesh mesh = new()
        {
            vertices = _vertices.ToArray(),
            triangles = _triangles.ToArray(),
            uv = _uvs.ToArray(),
            normals = _normals.ToArray()  
        };

        meshObject = new(blockID)
        {
            layer = LayerMask.NameToLayer("Collision")
        };
    
        meshObject.transform.parent = transform; 
        meshObject.transform.position = Game.Player.transform.position; 

        MeshFilter meshFilter = meshObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = meshObject.AddComponent<MeshRenderer>();
        meshFilter.mesh = mesh;
        Material meshMaterial = new(Resources.Load<Material>(BlockStatic.MESH_MATERIAL_PATH))
        {
            mainTexture = Resources.Load<Texture2D>($"texture/tileset/block_{blockID}")
        };

        // Set the rendering mode to Transparent
        meshMaterial.SetFloat("_Mode", 3);
        meshMaterial.SetFloat("_Surface", 1); // 1 means Transparent
        meshMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        meshMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        meshMaterial.SetInt("_ZWrite", 0);
        meshMaterial.DisableKeyword("_ALPHATEST_ON");
        meshMaterial.EnableKeyword("_ALPHABLEND_ON");
        meshMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        meshMaterial.renderQueue = 3000;

        // Set the alpha value to 0.5 for 50% transparency
        Color color = meshMaterial.color;
        color.a = OPACITY;
        meshMaterial.color = color;
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        

        meshRenderer.material = meshMaterial;

        return meshObject;
        //TODO 
        // AssetDatabase.CreateAsset(mesh, "Assets/Prefab/YourMesh.asset");
        // AssetDatabase.CreateAsset(_textureAtlas, "Assets/Prefab/YourTextureAtlas.asset");
        // AssetDatabase.CreateAsset(meshMaterial, "Assets/Prefab/YourMaterial.asset");
        // AssetDatabase.SaveAssets();
        // PrefabUtility.SaveAsPrefabAsset(meshObject, "Assets/Prefab/map.prefab");
 
    }

    void GenerateMesh()
    {
        _vertices = new List<Vector3>();
        _triangles = new List<int>();
        _uvs = new List<Vector2>();
        _normals = new List<Vector3>(); 
 
        AddFace(1, 36);//top
        AddFace(4, 84);//front
        AddFace(3, 84);//back
        AddFace(5, 84);//left
        AddFace(6, 84);//right
        AddFace(2, 33); // bottem 
    }

    void AddFace(int direction, int textureIndex)
    { 
        int vertexIndex = _vertices.Count;
        Vector3[] faceVertices = new Vector3[4];
        Vector3 normal = Vector3.zero;

        if (direction == 1) //top
        {
            faceVertices[0] = new Vector3(0, 1, 0);
            faceVertices[1] = new Vector3(1, 1, 0);
            faceVertices[2] = new Vector3(1, 1, 1);
            faceVertices[3] = new Vector3(0, 1, 1);
            _triangles.AddRange(new int[] { vertexIndex, vertexIndex + 2, vertexIndex + 1, vertexIndex, vertexIndex + 3, vertexIndex + 2 });
            normal = new Vector3(0, 1, 0);
        }
        else if (direction == 2) //down
        {
            faceVertices[0] = new Vector3(0, 0, 1);
            faceVertices[1] = new Vector3(1, 0, 1);
            faceVertices[2] = new Vector3(1, 0, 0);
            faceVertices[3] = new Vector3(0, 0, 0); 
            _triangles.AddRange(new int[] { vertexIndex, vertexIndex + 2, vertexIndex + 1, vertexIndex, vertexIndex + 3, vertexIndex + 2 });
            normal = new Vector3(0, -1, 0);
        }
        else if (direction == 3) //back
        {
            faceVertices[0] = new Vector3(0, 0, 1);
            faceVertices[1] = new Vector3(1, 0, 1);
            faceVertices[2] = new Vector3(1, 1, 1);
            faceVertices[3] = new Vector3(0, 1, 1);
            _triangles.AddRange(new int[] { vertexIndex, vertexIndex + 1, vertexIndex + 2, vertexIndex, vertexIndex + 2, vertexIndex + 3 });
            normal = new Vector3(0, 0, 1);
        }
        else if (direction == 4) //front
        {
            faceVertices[0] = new Vector3(0, 0, 0);
            faceVertices[1] = new Vector3(1, 0, 0);
            faceVertices[2] = new Vector3(1, 1, 0);
            faceVertices[3] = new Vector3(0, 1, 0);
            _triangles.AddRange(new int[] { vertexIndex, vertexIndex + 2, vertexIndex + 1, vertexIndex, vertexIndex + 3, vertexIndex + 2 });
            normal = new Vector3(0, 0, -1);
        }
        else if (direction == 5) //left
        {
            faceVertices[0] = new Vector3(0, 0, 0);
            faceVertices[1] = new Vector3(0, 0, 1);
            faceVertices[2] = new Vector3(0, 1, 1);
            faceVertices[3] = new Vector3(0, 1, 0);
            _triangles.AddRange(new int[] { vertexIndex, vertexIndex + 1, vertexIndex + 2, vertexIndex, vertexIndex + 2, vertexIndex + 3 });
            normal = new Vector3(-1, 0, 0);
        }
        else if (direction == 6) //right
        {
            faceVertices[0] = new Vector3(1, 0, 0);
            faceVertices[1] = new Vector3(1, 0, 1);
            faceVertices[2] = new Vector3(1, 1, 1);
            faceVertices[3] = new Vector3(1, 1, 0);
            _triangles.AddRange(new int[] { vertexIndex, vertexIndex + 2, vertexIndex + 1, vertexIndex, vertexIndex + 3, vertexIndex + 2 });
            normal = new Vector3(1, 0, 0);  
        }
        
        _vertices.AddRange(faceVertices); 
        for (int i = 0; i < 4; i++)
            _normals.Add(normal);

        Vector2Int tile = GetTileRect(textureIndex); 
        Vector2[] spriteUVs = new Vector2[] {
            new(tile.x / (float)textureWidth, tile.y / (float)textureHeight),
            new((tile.x + tileSize) / (float)textureWidth, tile.y / (float)textureHeight),
            new((tile.x + tileSize) / (float)textureWidth, (tile.y + tileSize) / (float)textureHeight),
            new(tile.x / (float)textureWidth, (tile.y + tileSize) / (float)textureHeight)
        };
        _uvs.AddRange(spriteUVs);
    }

    int textureWidth = 192;
    int textureHeight = 128;
    int tileSize = 16;
    int tilesPerRow = 12;
    int[] colx = new int[] {0, 16, 32, 48, 64, 80, 96, 112, 128, 144, 160, 176};
    int[] rowy = new int[] {112, 96, 80, 64, 48, 32, 16, 0};

    Vector2Int GetTileRect(int index)
    { 
        int targetRow = index / tilesPerRow;
        int targetCol = index % tilesPerRow;  

        return new Vector2Int(colx[targetCol], rowy[targetRow]);
    }




















}

