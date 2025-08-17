using System.Collections.Generic;
using UnityEngine;

public class BlockPreview 
{ 
    private static readonly Dictionary<string, (Mesh mesh, Material material)> Cache = new();

    private static readonly float Opacity = 0.45f;
    
    private static List<Vector3> _vertices;
    private static List<int> _triangles;
    private static List<Vector2> _uvs;
    private static List<Vector3> _normals;  
     
    public static void Set(GameObject gameObject, string blockID)
    {
        if (!Cache.TryGetValue(blockID, out var cached))
        {
            // Generate mesh
            GenerateMesh();
            Mesh mesh = new()
            {
                vertices = _vertices.ToArray(),
                triangles = _triangles.ToArray(),
                uv = _uvs.ToArray(),
                normals = _normals.ToArray()
            };

            // Create material
            Material meshMaterial = new(Resources.Load<Material>(Block.MeshMaterialPath))
            {
                mainTexture = Resources.Load<Texture2D>($"texture/tileset/block_{blockID}")
            };

            meshMaterial.SetFloat("_Mode", 3);
            meshMaterial.SetFloat("_Surface", 1);
            meshMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            meshMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            meshMaterial.SetInt("_ZWrite", 0);
            meshMaterial.DisableKeyword("_ALPHATEST_ON");
            meshMaterial.EnableKeyword("_ALPHABLEND_ON");
            meshMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            meshMaterial.renderQueue = 3000;

            Color color = meshMaterial.color;
            color.a = Opacity;
            meshMaterial.color = color;

            // Cache the result
            cached = (mesh, meshMaterial);
            Cache[blockID] = cached;
        }

        // Apply cached mesh and material
        GameObject sprite = gameObject.transform.Find("sprite").gameObject; 
        sprite.GetComponent<MeshFilter>().mesh = cached.mesh;
        sprite.GetComponent<MeshRenderer>().material = cached.material;
    }
   
    private static void GenerateMesh()
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

    private static void AddFace(int direction, int textureIndex)
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

        Vector2Int tile = Block.GetTileRect(textureIndex); 
        Vector2[] spriteUVs = new Vector2[] {
            new(tile.x / (float)Block.TextureWidth, tile.y / (float)Block.TextureHeight),
            new((tile.x + Block.TileSize) / (float)Block.TextureWidth, tile.y / (float)Block.TextureHeight),
            new((tile.x + Block.TileSize) / (float)Block.TextureWidth, (tile.y + Block.TileSize) / (float)Block.TextureHeight),
            new(tile.x / (float)Block.TextureWidth, (tile.y + Block.TileSize) / (float)Block.TextureHeight)
        };
        _uvs.AddRange(spriteUVs);
    }
}

