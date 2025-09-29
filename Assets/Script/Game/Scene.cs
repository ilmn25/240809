using System;
using System.Collections;
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
    public static Vector3Int PlayerChunkPosition;
    private static Vector3Int _playerChunkPositionPrevious;

    public static readonly int RenderRange = 2;
    public static readonly int LogicRange = 4; 
    public static readonly int GenRange = 7; 
    public static readonly int RenderDistance = RenderRange * World.ChunkSize; 
    public static readonly int LogicDistance = LogicRange * World.ChunkSize;
    public static bool Busy;
    public static void SwitchWorld(SaveData saveData)
    {  
        if (Busy) return;
        Busy = true;
        new CoroutineTask(Quit()).Finished += _ => {
            Save.LoadSave(saveData); 
            new CoroutineTask(Start()).Finished += _ => { Busy = false; };
        };
    }
    
    public static void LoadWorld()
    {  
        if (Busy) return;
        Busy = true;
        new CoroutineTask(Start()).Finished += _ => { Busy = false; };
    }
     
    
    private static IEnumerator Start()
    {
        Gen.Initialize();
        NavMap.Initialize();
        Control.SetPlayer(0); 
        Game.ViewPortObject.transform.position = Game.PlayerInfo.position;  
        _playerChunkPositionPrevious = Vector3Int.down; 
        yield return new WaitForSeconds(2);
        Game.SceneMode = SceneMode.Game;
        Environment.Target = EnvironmentType.Null;
    }
    private static IEnumerator Quit()
    {     
        Environment.Target = EnvironmentType.Black;
        yield return new WaitForSeconds(2);
        Game.SceneMode = SceneMode.Menu;
        if (World.Inst != null)
        {
            World.UnloadWorld();
            Helper.FileSave(World.Inst, Save.TempPath + SaveData.Inst.current); 
        }
        World.Inst = null;
    }
    public static void Update()
    { 
        PlayerChunkPosition = World.GetChunkCoordinate(Game.ViewPortObject.transform.position);
        if (PlayerChunkPosition != _playerChunkPositionPrevious)
        {
            CoroutineTask mapGenTask = new CoroutineTask(Gen.GenerateNearbyChunks(PlayerChunkPosition));
            if (Vector3.Distance(_playerChunkPositionPrevious, PlayerChunkPosition) > World.ChunkSize * 2)
                mapGenTask.Finished += (bool _) => { World.LoadWorld();};
            else World.LoadWorld();
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