using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockSystem : MonoBehaviour
{
    private static Dictionary<int, Block> _blockDefinitions;
    private static Map<int, string> _idMap;
    private static int _nextBlockID;
 
    
    public static Dictionary<int, Rect> _textureRectDictionary; // Dictionary to store the ID and corresponding Rect
    public static int _textureAtlasWidth;
    public static int _textureAtlasHeight;
    public static Texture2D _textureAtlas;
    public static Material _meshMaterial; 
    public static Material _shadowMeshMaterial; 
    
    void Awake()
    {
        _shadowMeshMaterial = new(Resources.Load<Material>("shader/material/custom_lit"));
        _meshMaterial = new(Resources.Load<Material>("shader/material/custom_lit"));
        _blockDefinitions = new Dictionary<int, Block>();
        _idMap = new Map<int, string>();
        _nextBlockID = 1;

        // Add block definitions
        AddBlockDefinition("brick", 1, 3, "Brick", "A block of brick");
        AddBlockDefinition("marble", 1, 5, "Marble", "A block of marble");
        AddBlockDefinition("stone", 1, 5, "Stone", "A block of stone");
        AddBlockDefinition("dirt", 1, 2, "Dirt", "A block of dirt");
        AddBlockDefinition("backroom", 1, 3, "Backroom", "A block of backroom");

        HandleTextureAtlas();
    }
 
    public void HandleTextureAtlas()
    {
        Rect[] _textureRects; 
        List<Texture2D> _textures = new List<Texture2D>();
        foreach (var kvp in _idMap.InttoString)
        {
            string stringID = kvp.Value;
            _textures.Add(Resources.Load<Texture2D>($"texture/tileset/block_{stringID}"));
        } 

        // Create texture atlas 
        //TODO 8192 4096 2048 if too small, uv mapping will BREAK
        int textureAtlasSize = (_idMap.InttoString.Count * 128) + 64; 
        _textureAtlas = new Texture2D(textureAtlasSize, textureAtlasSize);
        _textureAtlas.filterMode = FilterMode.Point;
        _textureRects = _textureAtlas.PackTextures(_textures.ToArray(), 0, textureAtlasSize); 

        // System.IO.File.WriteAllBytes(Application.dataPath + "/Resources/TextureAtlas.png", _textureAtlas.EncodeToPNG());  
        _textureAtlasWidth = _textureAtlas.width;
        _textureAtlasHeight = _textureAtlas.height;

        // Create the dictionary to pair the int ID with the texture rect
        _textureRectDictionary = new Dictionary<int, Rect>();
        int index = 0;
        foreach (var kvp in _idMap.InttoString)
        { 
            _textureRectDictionary[kvp.Key] = _textureRects[index];
            index++;
        }
        _meshMaterial.mainTexture = _textureAtlas;
    }

    private static void AddBlockDefinition(string stringID, int breakThreshold, int breakCost, string name, string description)
    {
        int id = _nextBlockID++;
        Block block = new Block(stringID, breakThreshold, breakCost, name, description);
        _blockDefinitions[id] = block;
        _idMap.Add(id, stringID);
    }

    public static Block GetBlockByID(int id)
    {
        if (_blockDefinitions.ContainsKey(id))
        {
            return _blockDefinitions[id];
        }
        return null;
    }

    public static Block GetBlockByStringID(string stringID)
    {
        int id = _idMap.StringtoInt[stringID];
        if (_blockDefinitions.ContainsKey(id))
        {
            return _blockDefinitions[id];
        }
        return null;
    }
 
    public static int GetIDByStringID(string stringID)
    {
        return _idMap.StringtoInt[stringID];
    }

    public static string GetStringIDByID(int id)
    {
        if (id == 0) return null;
        return _idMap.InttoString[id];
    }
}

[System.Serializable]
public class Block
{ 
    public string StringID { get; set; }
    public int BreakThreshold { get; set; }
    public int BreakCost { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    public Block(string stringID, int breakThreshold, int breakCost, string name, string description)
    { 
        StringID = stringID;
        BreakThreshold = breakThreshold;
        BreakCost = breakCost;
        Name = name;
        Description = description;
    }
} 

public class Map<T1, T2>
{
    private Dictionary<T1, T2> _intToString = new Dictionary<T1, T2>();
    private Dictionary<T2, T1> _stringToInt = new Dictionary<T2, T1>();

    public Map()
    {
        this.InttoString = new Indexer<T1, T2>(_intToString);
        this.StringtoInt = new Indexer<T2, T1>(_stringToInt);
    }

    public class Indexer<T3, T4> : IEnumerable<KeyValuePair<T3, T4>>
    {
        private Dictionary<T3, T4> _dictionary;
        public Indexer(Dictionary<T3, T4> dictionary)
        {
            _dictionary = dictionary;
        }
        public T4 this[T3 index]
        {
            get { return _dictionary[index]; }
            set { _dictionary[index] = value; }
        }

        public IEnumerator<KeyValuePair<T3, T4>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        public int Count => _dictionary.Count;
    }

    public void Add(T1 t1, T2 t2)
    {
        _intToString.Add(t1, t2);
        _stringToInt.Add(t2, t1);
    }

    public Indexer<T1, T2> InttoString { get; private set; }
    public Indexer<T2, T1> StringtoInt { get; private set; }
}