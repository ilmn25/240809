using System;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>An NPC that shows a one-line notice when its encounter spawns.</summary>
public interface IEncounterAnnounce
{
    string Announce { get; }
}

/// <summary>Scheduled world encounters, each an hourly roll: lone survivors and
/// travellers asking for goods during the day (Death Road to Canada style), the
/// mysterious visitor at the end of the night, and the nomad caravan every third
/// morning. Every entry owns its trigger rule, spawn distance and arrival message,
/// and at most one of each may be alive at a time. Survivors, petitioners and the
/// visitor call back into <see cref="MarkLeft"/> when they leave so the next roll can
/// happen.</summary>
public static class EncounterDirector
{
    public enum Kind { Survivor, Visitor, Caravan, Petitioner }

    private sealed class Encounter
    {
        public readonly Kind Type;
        private readonly float _spawnDistance;
        private readonly Func<int, int, bool> _isDue; // (hour, day)
        private readonly Func<ID> _roll;              // which entity to spawn
        private readonly string _message;             // shown on arrival; null = none

        private Info _active;

        public Encounter(Kind kind, float spawnDistance, Func<int, int, bool> isDue,
                         Func<ID> roll, string message = null)
        {
            Type = kind;
            _spawnDistance = spawnDistance;
            _isDue = isDue;
            _roll = roll;
            _message = message;
        }

        /// <summary>A survivor whose world was unloaded (or who died) no longer blocks the roll.</summary>
        public bool IsActive => _active != null && !_active.Destroyed && _active.Machine != null;

        public bool IsDue(int hour, int day) => _isDue(hour, day);

        public void Spawn()
        {
            _active = Entity.Spawn(_roll(), Event.SpawnPointAroundPlayer(_spawnDistance));

            if (_active?.Machine is IEncounterAnnounce announcer)
                Dialogue.ShowEvent(announcer.Announce);
            else if (_message != null)
                Dialogue.ShowEvent(_message);
        }

        public void MarkLeft() => _active = null;
    }

    private static readonly ID[] Survivors =
    {
        ID.WoundedDelver, ID.Deserter, ID.Pilgrim, ID.RenegadeCultist, ID.StarvingStranger,
        ID.BoundNPC,
    };

    private static readonly Encounter Visitor = new(
        Kind.Visitor, 12f,
        isDue: (hour, _) => hour == 22,
        roll: () => ID.Visitor);

    private static readonly Encounter Caravan = new(
        Kind.Caravan, 10f,
        isDue: (hour, day) => hour == 8 && day % 3 == 0,
        roll: () => ID.Caravan,
        message: "A band of nomads sets up camp nearby...");

    private static readonly Encounter Survivor = new(
        Kind.Survivor, 10f,
        isDue: (hour, _) => hour is >= 7 and <= 20 && Random.Range(0, 100) < 9,
        roll: () => Survivors[Random.Range(0, Survivors.Length)]);

    /// <summary>Daytime travellers who ask for goods and pay for them.</summary>
    private static readonly ID[] Petitioners =
    {
        ID.FamishedBeggar, ID.ScrapTinker, ID.ShrineHermit, ID.Hunter,
    };

    private static readonly Encounter Petitioner = new(
        Kind.Petitioner, 10f,
        isDue: (hour, _) => hour is >= 9 and <= 18 && Random.Range(0, 100) < 7,
        roll: () => Petitioners[Random.Range(0, Petitioners.Length)]);

    // Order matters: keeping the old subscriber order (visitor, caravan, survivor)
    // preserves the random stream; petitioners roll last so they don't shift it.
    private static readonly Encounter[] All = { Visitor, Caravan, Survivor, Petitioner };

    public static void Subscribe() => Environment.HourlyTriggered += OnHour;

    private static void OnHour(int hour, int day)
    {
        if (Main.PlayerInfo == null || Main.PlayerInfo.Destroyed) return;

        foreach (Encounter encounter in All)
        {
            if (encounter.IsActive) continue;
            if (encounter.IsDue(hour, day)) encounter.Spawn();
        }
    }

    /// <summary>Lets a fresh encounter of the same kind roll as soon as the current one is gone.</summary>
    public static void MarkLeft(Kind kind)
    {
        foreach (Encounter encounter in All)
            if (encounter.Type == kind) encounter.MarkLeft();
    }
}
