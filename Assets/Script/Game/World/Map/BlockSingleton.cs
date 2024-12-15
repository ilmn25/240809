using System.Collections.Generic;
using UnityEngine;

public class BlockSingleton : MonoBehaviour
{
    public static BlockSingleton Instance { get; private set; }  
    private static Dictionary<int, BlockData> _dictionary  = new Dictionary<int, BlockData>();
    private static IntStringMap<int, string> _blockIDMap = new IntStringMap<int, string>();
    private static int _nextBlockID = 1;
 
    
    public static Dictionary<int, Rect> TextureRectDictionary; // Dictionary to store the ID and corresponding Rect
    public static int TextureAtlasWidth;
    public static int TextureAtlasHeight;
    public static Texture2D TextureAtlas;
    
    public static Material MeshMaterial; 
    public static Material ShadowMeshMaterial;
    public const string MESH_MATERIAL_PATH = "shader/material/custom_lit";

    void Awake()
    {
        Instance = this;
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
        TextureRectDictionary = new Dictionary<int, Rect>();
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
}