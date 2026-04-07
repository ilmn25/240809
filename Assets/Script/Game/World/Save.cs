using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;

[Serializable]
public class Save
{
    public static Save Inst; 
    public readonly List<SaveData> List = new();
    public static string TempPath => "RunTime\\";  
    public const string WorldRuntimeFile = "World";
    public const string PlayerRuntimeFile = "Players";
    
    public static void Initialize()
    {
        Inst = Helper.FileLoad<Save>("Main");
        if (Inst == null)
        {
            Inst = new(); 
        }  
    }
    public static void Quit()
    {
        Helper.FileSave(Inst, "Main");
    }
    
    public static void NewSave(GenType gen)
    { 
        Helper.DeleteFolder(TempPath);
        Scene.SwitchWorld(new(gen));
    }
    
    public static void CloneSave()
    {
        _ = new CoroutineTask(CloneSaveCoroutine());
        IEnumerator CloneSaveCoroutine()
        {
            yield return new WaitForEndOfFrame();  
            SaveRuntimeState();
            SaveData data = (SaveData)Helper.Clone(SaveData.Inst);
            data.id = DateTime.Now.ToString("yyMMddHHmmss");
            Inst.List.Add(data);
            Helper.CloneFolder(TempPath, data.Path);
            Helper.SaveScreenShot(data.Path + "Preview");
            GUILoad.AddToList(Inst.List.Count - 1); 
            World.LoadWorld(); 
        }
    }
    
    public static void LoadSave(SaveData saveData)
    {   
        SaveData.Inst = (SaveData)Helper.Clone(saveData);
        Helper.CloneFolder(SaveData.Inst.Path, TempPath);
    } 

    public static void SaveRuntimeState()
    {
        if (World.Inst == null || SaveData.Inst == null) return;

        World.UnloadWorld();
        World.Inst.RemovePlayersFromChunks();

        Helper.FileSave(World.Inst, TempPath + WorldRuntimeFile);
        Helper.FileSave(BuildPlayerRuntimeData(), TempPath + PlayerRuntimeFile);
    }

    public static void LoadRuntimePlayers()
    {
        if (World.Inst == null || SaveData.Inst == null) return;

        World.Inst.target = new List<PlayerInfo>();

        PlayerRuntimeData playerData = Helper.FileLoad<PlayerRuntimeData>(TempPath + PlayerRuntimeFile);
        Vector3 spawnPosition = ResolveSpawnPosition(playerData);
        SaveData.Inst.spawnPosition = spawnPosition;

        if (playerData == null || playerData.players == null || playerData.players.Count == 0)
        {
            foreach (PlayerInfo player in CreateDefaultPlayers(spawnPosition))
            {
                QueuePlayerForSpawn(player, spawnPosition);
            }

            return;
        }

        foreach (PlayerInfo player in playerData.players)
        {
            QueuePlayerForSpawn(player, spawnPosition);
        }
    }

    private static void QueuePlayerForSpawn(PlayerInfo player, Vector3 spawnPosition)
    {
        player.position = spawnPosition;
        player.SpawnPoint = spawnPosition;
        World.Inst.target.Add(player);

        Vector3Int spawnChunk = World.GetChunkCoordinate(spawnPosition);
        if (World.Inst[spawnChunk] == null)
        {
            Gen.Generate(spawnChunk);
        }

        World.Inst[spawnChunk].DynamicEntity.Add(player);
    }

    private static List<PlayerInfo> CreateDefaultPlayers(Vector3 spawnPosition)
    {
        List<PlayerInfo> players = new List<PlayerInfo>();

        PlayerInfo first = (PlayerInfo)Entity.CreateInfo(ID.Player, spawnPosition);
        first.SpawnPoint = spawnPosition;
        players.Add(first);

        if (SaveData.Inst.current != GenType.Abyss)
        {
            return players;
        }

        PlayerInfo second = (PlayerInfo)Entity.CreateInfo(ID.Player, spawnPosition);
        second.SpawnPoint = spawnPosition;
        second.CharSprite = ID.Sheep;
        players.Add(second);

        PlayerInfo third = (PlayerInfo)Entity.CreateInfo(ID.Player, spawnPosition);
        third.SpawnPoint = spawnPosition;
        third.CharSprite = ID.Yuuri;
        players.Add(third);

        return players;
    }

    private static Vector3 ResolveSpawnPosition(PlayerRuntimeData playerData)
    {
        if (playerData != null)
        {
            return playerData.spawnPosition;
        }

        if (SaveData.Inst.spawnPosition != Vector3.zero)
        {
            return SaveData.Inst.spawnPosition;
        }

        return Gen.GetDefaultSpawnPosition();
    }

    private static PlayerRuntimeData BuildPlayerRuntimeData()
    {
        Vector3 spawnPosition = SaveData.Inst.spawnPosition;
        if (Main.PlayerInfo != null)
        {
            spawnPosition = Main.PlayerInfo.SpawnPoint;
        }

        PlayerRuntimeData data = new PlayerRuntimeData
        {
            spawnPosition = spawnPosition,
            players = new List<PlayerInfo>()
        };

        if (data.spawnPosition == Vector3.zero && World.Inst.target.Count > 0)
        {
            data.spawnPosition = World.Inst.target[0].SpawnPoint;
        }

        foreach (PlayerInfo player in World.Inst.target)
        {
            if (player == null) continue;
            data.players.Add(player);
        }

        return data;
    }
}

[Serializable]
public class SaveData
{
    public static SaveData Inst;
    public string Path => id + "\\";
    public string id;
    public int day = 1;
    public int time;
    public EnvironmentType weather = EnvironmentType.Sunrise;
    public GenType current;
    public Vector3 spawnPosition;

    public SaveData(){}
    public SaveData(GenType gen)
    {
        current = gen;
    }
}

[Serializable]
public class PlayerRuntimeData
{
    public Vector3 spawnPosition;
    public List<PlayerInfo> players = new();
}

