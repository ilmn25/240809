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

    public static void SwitchWorld(GenType genType, Vector3Int? spawnPoint = null)
    {  
        if (Busy) return;
        Busy = true;
        if (spawnPoint.HasValue)
        {
            Save.Inst.worlds[genType].SpawnPoint = spawnPoint.Value;
        }
        new CoroutineTask(Quit()).Finished += _ => {
            Save.Inst.current = genType;
            new CoroutineTask(Start()).Finished += _ => { Busy = false; };
        };
    }

    public static void SwitchSave(Save save)
    {
        if (Busy) return;
        Busy = true;
        new CoroutineTask(Quit()).Finished += _ => {
            Saves.LoadSave(save);
            new CoroutineTask(Start()).Finished += __ => { Busy = false; };
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
        Gen.Initialize(Save.Inst.current);
        Vector3 spawnPosition = World.Inst.SpawnPoint;
        Save.Inst.players.RemoveAll(player => player == null);
        foreach (PlayerInfo player in Save.Inst.players)
        {
            player.position = spawnPosition; 
            player.SpawnPoint = spawnPosition;
            if (player.Machine == null)
                Entity.SpawnFromInfo(player);
        }
        NavMap.Initialize();
        Control.SetPlayer(0); 
        _playerChunkPositionPrevious = Vector3Int.down; 
        yield return new WaitForSeconds(2);
        Main.SceneMode = SceneMode.Game;
        Environment.Target = EnvironmentType.Null;
    }
    private static IEnumerator Quit()
    {     
        Environment.Target = EnvironmentType.Black;
        yield return new WaitForSeconds(2);
        Main.SceneMode = SceneMode.Menu;
    }
    public static void Update()
    {  
        if (!Main.Player) return;
        PlayerChunkPosition = World.GetChunkCoordinate(Main.Player.transform.position);
        if (PlayerChunkPosition != _playerChunkPositionPrevious)
        {
            CoroutineTask mapGenTask = new CoroutineTask(Gen.GenerateNearbyChunks(PlayerChunkPosition));
            mapGenTask.Finished += (bool _) => { World.LoadWorld(); };
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
        Vector3 playerPos = Main.ViewPortObject.transform.position;

        return position.x >= playerPos.x - distance &&
               position.x <= playerPos.x + distance &&
               position.y >= playerPos.y - distance &&
               position.y <= playerPos.y + distance &&
               position.z >= playerPos.z - distance &&
               position.z <= playerPos.z + distance;
    }

}