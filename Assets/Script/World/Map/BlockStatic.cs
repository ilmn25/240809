using System.Collections.Generic;
using UnityEngine;

public class BlockStatic : MonoBehaviour
{
    public static BlockStatic Instance { get; private set; }  
    private static Dictionary<int, BlockData> _blockDefinitions  = new Dictionary<int, BlockData>();
    private static IntStringMap<int, string> _blockIDMap = new IntStringMap<int, string>();
    private static int _nextBlockID = 1;
 
    
    public static Dictionary<int, Rect> _textureRectDictionary; // Dictionary to store the ID and corresponding Rect
    public static int _textureAtlasWidth;
    public static int _textureAtlasHeight;
    public static Texture2D _textureAtlas;
    
    public static Material MeshMaterial; 
    public static Material ShadowMeshMaterial; 
    public static string MESH_MATERIAL_PATH = "shader/material/custom_lit";
    void Awake()
    {
        Instance = this;
        ShadowMeshMaterial = new(Resources.Load<Material>("shader/material/custom_shadow"));
        MeshMaterial = new(Resources.Load<Material>(MESH_MATERIAL_PATH));

        // Add block definitions
        AddBlockDefinition("brick", 1, 2, "Brick", "A block of brick");
        AddBlockDefinition("marble", 1, 1, "Marble", "A block of marble");
        AddBlockDefinition("stone", 1, 1, "Stone", "A block of stone");
        AddBlockDefinition("dirt", 1, 1, "Dirt", "A block of dirt");
        AddBlockDefinition("backroom", 1, 2, "Backroom", "A block of backroom");

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
        _textureAtlas = new Texture2D(textureAtlasSize, textureAtlasSize);
        _textureAtlas.filterMode = FilterMode.Point;
        _textureRects = _textureAtlas.PackTextures(_textures.ToArray(), 0, textureAtlasSize); 

        // System.IO.File.WriteAllBytes(Application.dataPath + "/Resources/TextureAtlas.png", _textureAtlas.EncodeToPNG());  
        _textureAtlasWidth = _textureAtlas.width;
        _textureAtlasHeight = _textureAtlas.height;

        // Create the dictionary to pair the int ID with the texture rect
        _textureRectDictionary = new Dictionary<int, Rect>();
        int index = 0;
        foreach (var kvp in _blockIDMap.InttoString)
        { 
            _textureRectDictionary[kvp.Key] = _textureRects[index];
            index++;
        }
        MeshMaterial.mainTexture = _textureAtlas;
    }

    private static void AddBlockDefinition(string stringID, int breakThreshold, int breakCost, string name, string description)
    {
        int id = _nextBlockID++;
        BlockData blockData = new BlockData(stringID, breakThreshold, breakCost, name, description);
        _blockDefinitions[id] = blockData;
        _blockIDMap.Add(id, stringID);
    }

    public static BlockData GetBlock(int id)
    {
        if (_blockDefinitions.ContainsKey(id))
        {
            return _blockDefinitions[id];
        }
        return null;
    }

    public static BlockData GetBlock(string stringID)
    {
        int id = _blockIDMap.StringtoInt[stringID];
        if (_blockDefinitions.ContainsKey(id))
        {
            return _blockDefinitions[id];
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