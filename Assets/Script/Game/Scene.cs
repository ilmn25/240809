using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Scene 
{ 
    public static Vector3Int PlayerChunkPosition;
    private static Vector3Int _playerChunkPositionPrevious;

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
        Vector3 spawnPosition = World.Inst.SpawnPoint;

        foreach (PlayerInfo player in Save.Inst.players)
            player.position = spawnPosition;

        // Initialise NavMap before any chunk data is written into it.
        NavMap.Initialize();

        if (Helper.IsHost())
        {
            // Generate ALL worlds up-front (Minecraft-style dimensions).
            // Gen.GenerateAllFor skips worlds that already have data.
            foreach (var kv in Save.Inst.worlds)
                Gen.GenerateAllFor(kv.Value);

            // Populate NavMap for the current (active) world.
            World.Inst.PopulateNavMap();

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

            // All chunks already exist — signal game ready immediately.
            Main.SceneMode = SceneMode.Game;
            Environment.Target = EnvironmentType.Null;
            Busy = false;
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
        GUIMain.OnGameEnd();
        Main.SceneMode = SceneMode.Menu;
    }
    public static void Update()
    {   
        if (!Main.Player) return;
        PlayerChunkPosition = World.GetChunkCoordinate(Main.Player.transform.position);

        if (Helper.IsHost())
        {
            // Always update render & entity state — handles player switches (Tab)
            // even when both players share the same chunk.
            World.LoadWorld();
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