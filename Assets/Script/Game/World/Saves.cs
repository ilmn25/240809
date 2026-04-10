using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;

[Serializable]
public class Saves
{
    public static Saves Inst; 
    public readonly List<Save> List = new();
    private const string SaveDataFile = "Save";
    
    public static void Initialize()
    {
        Inst = Helper.FileLoad<Saves>("SaveList");
        if (Inst == null)
        {
            Inst = new(); 
        }  
    }
    public static void Quit()
    {
        Helper.FileSave(Inst, "SaveList");
    }
    
    public static void SaveGame()
    {
        _ = new CoroutineTask(SaveGameCoroutine());
        IEnumerator SaveGameCoroutine()
        { 
            yield return new WaitForEndOfFrame();  
            World.UnloadWorld();
            Save data = Save.Inst;
            data.id = DateTime.Now.ToString("yyMMddHHmmssfff");
            Inst.List.Add(data);
            Helper.FileSave(data, data.Path + SaveDataFile);
            Helper.SaveScreenShot(data.Path + "Preview");
            GUILoad.AddToList(Inst.List.Count - 1); 
            World.LoadWorld(); 
        }
    }
    
    public static void LoadSave(Save save)
    {   
        Save.Inst = save.id == null? save : Helper.FileLoad<Save>(save.Path + SaveDataFile);
    } 
}

[Serializable]
public class Save
{
    public static Save Inst;
    public string Path => id + "\\";
    public string id;
    public int day = 1;
    public int time;
    public EnvironmentType weather = EnvironmentType.Sunrise;
    public GenType current;
    public int seed;
    public List<PlayerInfo> players = new();
    public Dictionary<GenType, World> worlds = new();

    public Save(){}
    public Save(GenType gen)
    {
        current = gen;
        seed = UnityEngine.Random.Range(1, 1000000);

        players = new List<PlayerInfo>();
        worlds = new Dictionary<GenType, World>()
        {
            { GenType.Abyss, new World(GenType.Abyss) },
            { GenType.SkyBlock, new World(GenType.SkyBlock) },
            { GenType.SuperFlat, new World(GenType.SuperFlat) }
        };

        Vector3 spawnPosition = worlds[gen].SpawnPoint;

        PlayerInfo first = (PlayerInfo)Entity.CreateInfo(ID.Player, spawnPosition);
        first.SpawnPoint = spawnPosition;
        players.Add(first);

        if (gen != GenType.Abyss)
        {
            return;
        }

        PlayerInfo second = (PlayerInfo)Entity.CreateInfo(ID.Player, spawnPosition);
        second.SpawnPoint = spawnPosition;
        second.CharSprite = ID.Sheep;
        players.Add(second);

        PlayerInfo third = (PlayerInfo)Entity.CreateInfo(ID.Player, spawnPosition);
        third.SpawnPoint = spawnPosition;
        third.CharSprite = ID.Yuuri;
        players.Add(third);
    }
}

