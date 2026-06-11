using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Mirror;
using UnityEngine;
using Debug = UnityEngine.Debug;


public class Scene 
{ 
    public static Vector3Int PlayerChunkPosition;
    private static Vector3Int _playerChunkPositionPrevious;
    private static readonly Dictionary<int, Vector3Int> _playerChunkPositions = new();
    private static bool _hostFirstGenDone;

    public static readonly int RenderRange = 2;
    public static readonly int LogicRange = 3; 
    public static readonly int GenRange = 4; 
    public static readonly int RenderDistance = RenderRange * World.ChunkSize; 
    public static readonly int LogicDistance = LogicRange * World.ChunkSize;
    public static bool Busy;

    public static void SwitchWorld(GenType genType, Vector3Int? spawnPoint = null)
    {  
        if (Busy) return;
        Busy = true;
        if (spawnPoint.HasValue)
            Save.Inst.worlds[genType].SpawnPoint = spawnPoint.Value;
        new CoroutineTask(Quit(false)).Finished += _ => {
            Save.Inst.current = genType;
            Start(); 
        };
    }

    public static void SwitchSave(Save save)
    {
        if (Busy) return;
        Busy = true;
        new CoroutineTask(Quit(true)).Finished += _ => {
            Saves.LoadSave(save);
            Start();
        };
    }
    
    public static void LoadWorld()
    {  
        if (Busy) return;
        Busy = true;
        Start();
    }
    
    private static void Start()
    {
        Gen.Initialize(Save.Inst.current);
        Vector3 spawnPosition = World.Inst.SpawnPoint;

        foreach (PlayerInfo player in Save.Inst.players)
            player.position = spawnPosition;

        if (Helper.IsHost())
        {
            _playerChunkPositions.Clear();
            _hostFirstGenDone = false;
            foreach (PlayerInfo player in Save.Inst.players)
            {
                if (player.Machine == null) Entity.SpawnFromInfo(player, false);
                player.Machine.transform.position = spawnPosition;
            }
            _playerChunkPositionPrevious = Vector3Int.down;
        }
        else
        {
            // Remote client: chunks come from the server.
            PlayerChunkPosition = World.GetChunkCoordinate(Save.Inst.players[0].position);
            _playerChunkPositionPrevious = PlayerChunkPosition;
            World.LoadWorld();
            Busy = false;
        }

        NavMap.Initialize();
        if (!Helper.IsHost())
        {
            // Build NavMap for chunks received from server in starting range.
            Vector3Int center = World.GetChunkCoordinate(Save.Inst.players[0].position);
            for (int x = -GenRange; x <= GenRange; x++)
                for (int y = -GenRange; y <= GenRange; y++)
                    for (int z = -GenRange; z <= GenRange; z++)
                        NavMap.SetChunk(new Vector3Int(
                            center.x + x * World.ChunkSize,
                            center.y + y * World.ChunkSize,
                            center.z + z * World.ChunkSize));
        }
        Control.SetPlayer(0);
        if (Helper.IsHost())
        {
            PlayerInfo firstPlayer = global::Save.Inst.players[0];
            PlayerSync.HostClaimPlayer(firstPlayer.uid);
        }
    }
    private static IEnumerator Quit(bool includePlayers)
    {     
        Environment.Target = EnvironmentType.Black; 
        yield return new WaitForSeconds(2);
        if (includePlayers && Save.Inst != null)
            foreach (PlayerInfo player in Save.Inst.players)
                if (player.Machine != null)
                    ObjectPool.ReturnObject(player.Machine.gameObject);
        World.UnloadWorld();
        Main.SceneMode = SceneMode.Menu;
    }
    public static void Update()
    {   
        if (!Main.Player) return;
        PlayerChunkPosition = World.GetChunkCoordinate(Main.Player.transform.position);

        if (Helper.IsHost())
        {
            for (int i = 0; i < Save.Inst.players.Count; i++)
            {
                PlayerInfo p = Save.Inst.players[i];
                if (p.Machine == null || p.ownerId == -1) continue;
                Vector3Int chunkPos = World.GetChunkCoordinate(p.position);
                if (_playerChunkPositions.TryGetValue(i, out var prev) && prev == chunkPos) continue;
                _playerChunkPositions[i] = chunkPos;

                var genTask = new CoroutineTask(Gen.GenerateNearbyChunks(chunkPos, GenRange));
                if (!_hostFirstGenDone)
                {
                    genTask.Finished += (bool _) => {
                        if (!_hostFirstGenDone)
                        {
                            _hostFirstGenDone = true;
                            Main.SceneMode = SceneMode.Game;
                            Environment.Target = EnvironmentType.Null;
                            Busy = false;
                        }
                    };
                }
                genTask.Finished += (bool _) => { World.LoadWorld(); };
            }
            // Update rendering when the controlled player moves or host switches players
            if (PlayerChunkPosition != _playerChunkPositionPrevious)
            {
                World.LoadWorld();
                _playerChunkPositionPrevious = PlayerChunkPosition;
            }
        }
        else if (PlayerChunkPosition != _playerChunkPositionPrevious)
        {
            World.LoadWorld();
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