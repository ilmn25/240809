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
    /// <summary>The phrase the player typed for the seed, if any (letters only).</summary>
    public string seedText;
    /// <summary>How many mobs spawn and whether hostiles spawn at all.</summary>
    public MobDifficulty difficulty = MobDifficulty.Normal;
    /// <summary>How much scenery the generator scatters over the surface.</summary>
    public WorldDensity density = WorldDensity.Normal;
    /// <summary>Real-time length of a full in-game day.</summary>
    public DayLength dayLength = DayLength.Normal;
    public QuestState quest = new();
    public List<PlayerInfo> players = new();
    public Dictionary<GenType, World> worlds = new();

    /// <summary>Which floor of the dungeon the current run is on (1 = first floor below
    /// the surface). Reset whenever a run starts from the surface.</summary>
    public int dungeonDepth = 1;
    /// <summary>Clamped floor number: saves written before floors existed read back as 0.</summary>
    public int DungeonDepth => dungeonDepth < 1 ? 1 : dungeonDepth;

    // The Maw's daily coin quota state (see MawQuota).
    public int mawQuota = MawQuota.BaseQuota;
    public int mawPaid;
    public int mawDay = 1;
    public int mawStreak;
    public bool mawUnlocked;

    public Save(){}
    public Save(GenType gen) : this(gen, NewGameOptions.Default) {}

    /// <param name="gen">World type the save starts in.</param>
    /// <param name="options">Choices made on the New Game screen.</param>
    public Save(GenType gen, NewGameOptions options)
    {
        current = gen;
        seed = options.Seed ?? UnityEngine.Random.Range(1, 1000000);
        seedText = options.SeedText;
        difficulty = options.Difficulty;
        density = options.Density;
        dayLength = options.DayLength;

        players = new List<PlayerInfo>();
        worlds = new Dictionary<GenType, World>()
        {
            { GenType.Abyss, new World(GenType.Abyss) },
            { GenType.SuperFlat, new World(GenType.SuperFlat) },
            { GenType.Backrooms, new World(GenType.Backrooms) },
            { GenType.Dungeon, new World(GenType.Dungeon) },
            { GenType.Edit, new World(GenType.Edit) },
            { GenType.Maw, new World(GenType.Maw) }
        };

        Vector3 spawnPosition = worlds[gen].SpawnPoint;

        for (int i = 0; i < Mathf.Clamp(options.Delvers, 1, 3); i++)
        {
            PlayerInfo player = (PlayerInfo)Entity.CreateInfo(ID.Player, spawnPosition);
            if (i == 1) player.CharSprite = ID.Yuuri; // the second delver wears the Yuuri sprite
            players.Add(player);
        }
    }
}

/// <summary>Everything the New Game screen lets the player choose. Kept as one value so
/// <see cref="Save"/>'s constructor doesn't grow an ever-longer parameter list.</summary>
public struct NewGameOptions
{
    /// <summary>Fixed world seed; null picks a random one.</summary>
    public int? Seed;
    /// <summary>Phrase the player typed for the seed, shown in save listings.</summary>
    public string SeedText;
    /// <summary>How many players start in this world (1-3).</summary>
    public int Delvers;
    public MobDifficulty Difficulty;
    public WorldDensity Density;
    public DayLength DayLength;

    public static NewGameOptions Default => new()
    {
        Delvers = 2,
        Difficulty = MobDifficulty.Normal,
        Density = WorldDensity.Normal,
        DayLength = DayLength.Normal,
    };
}

