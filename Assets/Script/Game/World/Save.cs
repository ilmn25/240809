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

        if (SaveData.Inst.players == null)
        {
            SaveData.Inst.players = new List<PlayerInfo>();
        }
        SaveData.Inst.players.RemoveAll(player => player == null);

        if (Main.PlayerInfo != null)
        {
            World.Inst.SpawnPoint = Vector3Int.FloorToInt(Main.PlayerInfo.SpawnPoint);
        }
        else if (World.Inst.SpawnPoint == Vector3Int.zero && SaveData.Inst.players.Count > 0)
        {
            World.Inst.SpawnPoint = Vector3Int.FloorToInt(SaveData.Inst.players[0].SpawnPoint);
        }

        Helper.FileSave(World.Inst, TempPath + WorldRuntimeFile);
    }

    public static void LoadRuntimePlayers()
    {
        if (World.Inst == null || SaveData.Inst == null) return;
        List<PlayerInfo> loadedPlayers = SaveData.Inst.players ?? new List<PlayerInfo>();
        loadedPlayers.RemoveAll(player => player == null);
        if (World.Inst.SpawnPoint == Vector3Int.zero)
        {
            World.Inst.SpawnPoint = Gen.Dictionary[SaveData.Inst.current].SpawnPoint;
        }

        Vector3 spawnPosition = World.Inst.SpawnPoint;

        SaveData.Inst.players = new List<PlayerInfo>();

        if (loadedPlayers.Count == 0)
        {
            foreach (PlayerInfo player in CreateDefaultPlayers(spawnPosition))
            {
                QueuePlayerForSpawn(player, spawnPosition);
            }

            return;
        }

        foreach (PlayerInfo player in loadedPlayers)
        {
            QueuePlayerForSpawn(player, spawnPosition);
        }
    }

    private static void QueuePlayerForSpawn(PlayerInfo player, Vector3 spawnPosition)
    {
        player.position = spawnPosition;
        player.SpawnPoint = spawnPosition;
        SaveData.Inst.players.Add(player);

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
    public List<PlayerInfo> players = new();

    public SaveData(){}
    public SaveData(GenType gen)
    {
        current = gen;
    }
}

