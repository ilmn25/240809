using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using Debug = UnityEngine.Debug;


public class WorldSingleton : MonoBehaviour
{ 
    public static WorldSingleton Instance { get; private set; }

    public static event Action PlayerChunkTraverse; 

    public static Vector3Int PlayerChunkPosition;
    private static Vector3Int _playerChunkPositionPrevious;

    public static readonly int RenderRange = 2;
    public static readonly int LogicRange = 3; 
    public static readonly int RenderDistance = RenderRange * World.ChunkSize; 
    public static readonly int LogicDistance = LogicRange * World.ChunkSize; 
    
    public static bool AlwaysRegenerate = false;
    public string setPieceName = "tree_a";
    private void Awake()    
    {
        Instance = this;
    }
 
    void Start()
    { 
        Instance = this;
 
        if (!File.Exists(World.GetFilePath(0)) || AlwaysRegenerate) 
            WorldGenSingleton.Instance.Generate();
        else
            World.Load(0); 
        
        _playerChunkPositionPrevious = World.GetChunkCoordinate(Game.Player.transform.position); 
    }
          
    void FixedUpdate()
    {
        PlayerChunkPosition = World.GetChunkCoordinate(Game.Player.transform.position);
        if (PlayerChunkPosition != _playerChunkPositionPrevious)
        {  
            PlayerChunkTraverse?.Invoke();
            _playerChunkPositionPrevious = PlayerChunkPosition; 
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            World.Save(0);
            Application.Quit();
        }
    }

    public static bool InPlayerRange(Vector3 position, float distance)
    {
        return Math.Abs(position.x - PlayerChunkPosition.x) <= distance &&
               Math.Abs(position.y - PlayerChunkPosition.y) <= distance &&
               Math.Abs(position.z - PlayerChunkPosition.z) <= distance;
    }
}