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
        SaveData.Inst = new SaveData(gen);
        Scene.SwitchWorld(gen);
    }
    
    public static void CloneSave()
    {
        _ = new CoroutineTask(CloneSaveCoroutine());
        IEnumerator CloneSaveCoroutine()
        {
            yield return new WaitForEndOfFrame();  
            SaveData data = (SaveData)Helper.Clone(SaveData.Inst);
            data.id = DateTime.Now.ToString("yyMMddHHmmss");
            Inst.List.Add(data);
            Helper.SaveScreenShot(data.Path + "Preview");
            GUILoad.AddToList(Inst.List.Count - 1); 
            World.LoadWorld(); 
        }
    }
    
    public static void LoadSave(SaveData saveData)
    {   
        SaveData.Inst = (SaveData)Helper.Clone(saveData);
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
    public int seed;
    public List<PlayerInfo> players = new();
    public Dictionary<GenType, World> worlds = new();

    public SaveData(){}
    public SaveData(GenType gen)
    {
        current = gen;
        seed = UnityEngine.Random.Range(1, 1000000);
        Vector3 spawnPosition = Vector3.zero;

        players = new List<PlayerInfo>();
        worlds = new Dictionary<GenType, World>()
        {
            { GenType.Abyss, new World(GenType.Abyss) },
            { GenType.SkyBlock, new World(GenType.SkyBlock) },
            { GenType.SuperFlat, new World(GenType.SuperFlat) }
        };

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

