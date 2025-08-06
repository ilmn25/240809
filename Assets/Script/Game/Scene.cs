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


public class Scene 
{ 
    public static event Action PlayerChunkTraverse; 

    public static Vector3Int PlayerChunkPosition;
    private static Vector3Int _playerChunkPositionPrevious;

    public static readonly int RenderRange = 2;
    public static readonly int LogicRange = 2; 
    public static readonly int RenderDistance = RenderRange * World.ChunkSize; 
    public static readonly int LogicDistance = LogicRange * World.ChunkSize;

    public const string SetPieceName = "tree_a";

    public static void Initialize()
    {  
        World.Load(0); 
        _playerChunkPositionPrevious = World.GetChunkCoordinate(Game.Player.transform.position); 
    }

    public static void Update()
    {
        PlayerChunkPosition = World.GetChunkCoordinate(Game.Player.transform.position);
        if (PlayerChunkPosition != _playerChunkPositionPrevious)
        {  
            PlayerChunkTraverse?.Invoke();
            _playerChunkPositionPrevious = PlayerChunkPosition; 
        }
    }
 
    public static bool InPlayerChunkRange(Vector3 position, float distance)
    {
        return Math.Abs(position.x - PlayerChunkPosition.x) <= distance &&
               Math.Abs(position.y - PlayerChunkPosition.y) <= distance &&
               Math.Abs(position.z - PlayerChunkPosition.z) <= distance;
    }
    
    public static bool InPlayerBlockRange(Vector3 position, float distance)
    { 
        if (Mathf.Abs(position.x - Game.Player.transform.position.x) > distance ||
            Mathf.Abs(position.y - Game.Player.transform.position.y) > distance ||
            Mathf.Abs(position.z - Game.Player.transform.position.z) > distance)
        {
            return false;
        }
        return true;
    }
}