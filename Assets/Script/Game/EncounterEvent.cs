using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>Random recruitable-NPC encounters, Death Road to Canada style. During the
/// day an hourly roll sometimes sends a lone survivor to hail the player. They wait
/// nearby and offer to join; left alone they move on. Only one encounter is active at a
/// time, and the survivor type is rolled fresh each time.</summary>
public static class EncounterEvent
{
    private const int FirstHour = 7;          // nobody hails you before morning
    private const int LastHour = 20;          // or after dusk
    private const int ChancePercent = 9;      // per eligible hour
    private const float SpawnDistance = 10f;  // how far from the player they appear

    /// <summary>Every survivor that can turn up; each spawn rolls one at random.</summary>
    private static readonly ID[] Survivors =
    {
        ID.WoundedDelver, ID.Deserter, ID.Pilgrim, ID.RenegadeCultist, ID.StarvingStranger,
    };

    private static Info _active;

    public static void Subscribe() => Environment.HourlyTriggered += OnHour;

    /// <summary>Lets a fresh encounter roll as soon as the current one is gone.</summary>
    public static void MarkLeft() => _active = null;

    private static void OnHour(int hour, int day)
    {
        if (hour < FirstHour || hour > LastHour) return;
        if (Main.PlayerInfo == null || Main.PlayerInfo.Destroyed) return;
        // A survivor whose world was unloaded (or who died) no longer blocks the roll.
        if (_active != null && !_active.Destroyed && _active.Machine != null) return;
        if (Random.Range(0, 100) >= ChancePercent) return;

        Spawn();
    }

    private static void Spawn()
    {
        ID survivor = Survivors[Random.Range(0, Survivors.Length)];
        _active = Entity.Spawn(survivor, Event.SpawnPointAroundPlayer(SpawnDistance));
        if (_active?.Machine is RecruitableNPCMachine npc)
            Dialogue.ShowEvent(npc.Announce);
    }
}
