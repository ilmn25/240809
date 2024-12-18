using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class Block 
{
    private static Dictionary<int, BlockData> _dictionary  = new Dictionary<int, BlockData>();
    private static IntStringMap<int, string> _blockIDMap = new IntStringMap<int, string>();
    private static int _nextBlockID = 1;
    
    public static int TextureAtlasWidth;
    public static int TextureAtlasHeight;
    public static readonly int TextureWidth = 192;
    public static readonly int TextureHeight = 128;
    public static readonly int TileSize = 16;
    public static readonly int TilesPerRow = 12;
    public static Texture2D TextureAtlas; 
    public static NativeArray<int> Colx;
    public static NativeArray<int> Rowy;
    public static NativeHashMap<int, Rect> TextureRectDictionary;
    
    public static Material MeshMaterial; 
    public static Material ShadowMeshMaterial;
    public const string MeshMaterialPath = "shader/material/custom_lit";

    public static void Initialize()
    {
        Colx = new NativeArray<int>(new[] {0, 16, 32, 48, 64, 80, 96, 112, 128, 144, 160, 176}, Allocator.Persistent);
        Rowy = new NativeArray<int>(new[] {112, 96, 80, 64, 48, 32, 16, 0}, Allocator.Persistent);  
        
        ShadowMeshMaterial = new(Resources.Load<Material>("shader/material/custom_shadow"));
        MeshMaterial = new(Resources.Load<Material>(MeshMaterialPath));
 
        HandleTextureAtlas();
    }

    public static void Dispose()
    {
        Colx.Dispose();
        Rowy.Dispose();
        TextureRectDictionary.Dispose();
    }
    
    private static void HandleTextureAtlas()
    {
        List<Texture2D> textures = new List<Texture2D>();
        foreach (var kvp in _blockIDMap.InttoString)
        {
            string stringID = kvp.Value;
            textures.Add(Resources.Load<Texture2D>($"texture/tileset/block_{stringID}"));
        } 

        // if too small, uv mapping will BREAK
        int textureAtlasSize = _blockIDMap.InttoString.Count * TextureHeight + 64; 
        TextureAtlas = new Texture2D(textureAtlasSize, textureAtlasSize)
        {
            filterMode = FilterMode.Point
        };
        Rect[] textureRects = TextureAtlas.PackTextures(textures.ToArray(), 0, textureAtlasSize); 

        TextureAtlasWidth = TextureAtlas.width;
        TextureAtlasHeight = TextureAtlas.height;

        TextureRectDictionary = new NativeHashMap<int, Rect>(_blockIDMap.InttoString.Count, Allocator.Persistent);
        int index = 0;
        foreach (var kvp in _blockIDMap.InttoString)
        { 
            TextureRectDictionary[kvp.Key] = textureRects[index];
            index++;
        }
        MeshMaterial.mainTexture = TextureAtlas;
    }

    public static void AddBlockDefinition(string stringID, int breakThreshold, int breakCost)
    {
        int id = _nextBlockID++;
        BlockData blockData = new BlockData(stringID, breakThreshold, breakCost);
        _dictionary[id] = blockData;
        _blockIDMap.Add(id, stringID);
    }

    public static BlockData GetBlock(int id)
    {
        return _dictionary.GetValueOrDefault(id);
    }

    public static BlockData GetBlock(string stringID)
    {
        return _dictionary.GetValueOrDefault(_blockIDMap.StringtoInt[stringID]);
    }
 
    public static int ConvertID(string stringID)
    {
        return _blockIDMap.StringtoInt[stringID];
    }

    public static string ConvertID(int id)
    {
        if (id == 0) return null;
        return _blockIDMap.InttoString[id];
    }
 
    public static Vector2Int GetTileRect(int index)
    { 
        int targetRow = index / TilesPerRow;
        int targetCol = index % TilesPerRow;  

        return new Vector2Int(Colx[targetCol], Rowy[targetRow]);
    }
}