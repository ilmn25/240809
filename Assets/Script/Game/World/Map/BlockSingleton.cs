using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class BlockSingleton : MonoBehaviour
{
    public static BlockSingleton Instance { get; private set; }  
    private static Dictionary<int, BlockData> _dictionary  = new Dictionary<int, BlockData>();
    private static IntStringMap<int, string> _blockIDMap = new IntStringMap<int, string>();
    private static int _nextBlockID = 1;
 
    
    public static int TextureAtlasWidth;
    public static int TextureAtlasHeight;
    public static Texture2D TextureAtlas;
    public static int TileSize;
    public static int TilesPerRow;
    public static NativeArray<int> Colx;
    public static NativeArray<int> Rowy;
    public static NativeHashMap<int, Rect> TextureRectDictionary;
    
    public static Material MeshMaterial; 
    public static Material ShadowMeshMaterial;
    public const string MESH_MATERIAL_PATH = "shader/material/custom_lit";

    void Awake()
    {
        Instance = this;
        
        TileSize = 16;
        TilesPerRow = 12;
        int[] colx = new int[] {0, 16, 32, 48, 64, 80, 96, 112, 128, 144, 160, 176};
        int[] rowy = new int[] {112, 96, 80, 64, 48, 32, 16, 0};
        Colx = new NativeArray<int>(colx, Allocator.Persistent);
        Rowy = new NativeArray<int>(rowy, Allocator.Persistent);  
        
        ShadowMeshMaterial = new(Resources.Load<Material>("shader/material/custom_shadow"));
        MeshMaterial = new(Resources.Load<Material>(MESH_MATERIAL_PATH));
 
        HandleTextureAtlas();
    }
 
    public void HandleTextureAtlas()
    {
        Rect[] _textureRects; 
        List<Texture2D> _textures = new List<Texture2D>();
        foreach (var kvp in _blockIDMap.InttoString)
        {
            string stringID = kvp.Value;
            _textures.Add(Resources.Load<Texture2D>($"texture/tileset/block_{stringID}"));
        } 

        // Create texture atlas 
        //TODO 8192 4096 2048 if too small, uv mapping will BREAK
        int textureAtlasSize = (_blockIDMap.InttoString.Count * 128) + 64; 
        TextureAtlas = new Texture2D(textureAtlasSize, textureAtlasSize);
        TextureAtlas.filterMode = FilterMode.Point;
        _textureRects = TextureAtlas.PackTextures(_textures.ToArray(), 0, textureAtlasSize); 

        // System.IO.File.WriteAllBytes(Application.dataPath + "/Resources/TextureAtlas.png", _textureAtlas.EncodeToPNG());  
        TextureAtlasWidth = TextureAtlas.width;
        TextureAtlasHeight = TextureAtlas.height;

        // Create the dictionary to pair the int ID with the texture rect
        TextureRectDictionary = new NativeHashMap<int, Rect>(_blockIDMap.InttoString.Count, Allocator.Persistent);
        int index = 0;
        foreach (var kvp in _blockIDMap.InttoString)
        { 
            TextureRectDictionary[kvp.Key] = _textureRects[index];
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
        if (_dictionary.ContainsKey(id))
        {
            return _dictionary[id];
        }
        return null;
    }

    public static BlockData GetBlock(string stringID)
    {
        int id = _blockIDMap.StringtoInt[stringID];
        if (_dictionary.ContainsKey(id))
        {
            return _dictionary[id];
        }
        return null;
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

    public static int textureWidth = 192;
    public static int textureHeight = 128;

    public static Vector2Int GetTileRect(int index)
    { 
        int targetRow = index / BlockSingleton.TilesPerRow;
        int targetCol = index % BlockSingleton.TilesPerRow;  

        return new Vector2Int(BlockSingleton.Colx[targetCol], BlockSingleton.Rowy[targetRow]);
    }
}