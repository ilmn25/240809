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
            World.LoadWorld(); 
        }
    }
    
    public static void LoadSave(Save save)
    {   
        Save.Inst = save.id == null ? save : (Helper.FileLoad<Save>(save.Path + SaveDataFile) ?? save);
        Main.PlayerInfo = Save.Inst.players[0];
    }

    /// <summary>Ensures a World exists for every GenType (fixes saves made before
    /// a new world type was added, so e.g. the Maw is always present).</summary>
    public static void EnsureWorlds()
    {
        if (Save.Inst == null) return;
        foreach (GenType gen in System.Enum.GetValues(typeof(GenType)))
            if (!Save.Inst.worlds.ContainsKey(gen))
                Save.Inst.worlds[gen] = new World(gen);
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
    public QuestState quest = new();
    public List<PlayerInfo> players = new();
    public Dictionary<GenType, World> worlds = new();

    // The Maw's daily gold quota state (see MawQuota).
    public int mawQuota = 15;
    public int mawPaid;
    public int mawDay = 1;
    public int mawStreak;
    public bool mawUnlocked;

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
            { GenType.SuperFlat, new World(GenType.SuperFlat) },
            { GenType.Backrooms, new World(GenType.Backrooms) },
            { GenType.Dungeon, new World(GenType.Dungeon) },
            { GenType.Edit, new World(GenType.Edit) },
            { GenType.Maw, new World(GenType.Maw) }
        };

        Vector3 spawnPosition = worlds[gen].SpawnPoint;

        PlayerInfo first = (PlayerInfo)Entity.CreateInfo(ID.Player, spawnPosition);
        players.Add(first);
 
        PlayerInfo third = (PlayerInfo)Entity.CreateInfo(ID.Player, spawnPosition);
        third.CharSprite = ID.Yuuri;
        players.Add(third);
    }
}

