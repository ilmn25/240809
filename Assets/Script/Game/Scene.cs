using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;


public class Scene 
{ 
    public static event Action PlayerChunkTraverse; 

    public static Vector3Int PlayerChunkPosition;
    private static Vector3Int _playerChunkPositionPrevious;

    public static readonly int RenderRange = 2;
    public static readonly int LogicRange = 5; 
    public static readonly int RenderDistance = RenderRange * World.ChunkSize; 
    public static readonly int LogicDistance = LogicRange * World.ChunkSize;

    public const string SetPieceName = "tree_a";

    public static void Initialize()
    {  
        World.Load(0); 
        Control.SetPlayer(0); 
        Game.ViewPortObject.transform.position = Game.PlayerInfo.position;
        GUIHealthBar.Initialize();
    }
    

    public static void Update()
    {
        PlayerChunkPosition = World.GetChunkCoordinate(Game.ViewPortObject.transform.position);
        if (PlayerChunkPosition != _playerChunkPositionPrevious)
        {  
            PlayerChunkTraverse?.Invoke();
            _playerChunkPositionPrevious = PlayerChunkPosition; 
        }
    }
    
    public static bool InPlayerChunkRange(Vector3 position, float distance)
    {
        return position.x >= PlayerChunkPosition.x - distance &&
               position.x <= PlayerChunkPosition.x + distance + 1 &&
               position.y >= PlayerChunkPosition.y - distance &&
               position.y <= PlayerChunkPosition.y + distance + 1 &&
               position.z >= PlayerChunkPosition.z - distance &&
               position.z <= PlayerChunkPosition.z + distance + 1;
    }

    public static bool InPlayerBlockRange(Vector3 position, float distance)
    {
        Vector3 playerPos = Game.ViewPortObject.transform.position;

        return position.x >= playerPos.x - distance &&
               position.x <= playerPos.x + distance &&
               position.y >= playerPos.y - distance &&
               position.y <= playerPos.y + distance &&
               position.z >= playerPos.z - distance &&
               position.z <= playerPos.z + distance;
    }

}