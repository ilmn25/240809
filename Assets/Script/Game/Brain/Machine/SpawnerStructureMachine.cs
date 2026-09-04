using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base for structures that keep a fixed number of transient units (NPCs or mobs)
/// around them — the Guide by the owl statue, raiders round a dirty tent, a hive's
/// hornets, etc. Units aren't saved, so the structure restores them when it loads
/// and again at the start of each new day.
///
/// There's no per-frame respawn timer to tune: the population is only topped up on
/// (re)load and once per day, so a unit that dies mid-day comes back the next day
/// (or the next time the structure is loaded).
/// </summary>
public abstract class SpawnerStructureMachine : StructureMachine
{
    /// <summary>How many units this structure keeps around it. Single-"home"
    /// structures (owl statue, tents) keep 1; swarms override this higher.</summary>
    protected virtual int MaxUnits => 1;

    private readonly List<Info> _units = new List<Info>();
    private int _seenDay = int.MinValue;

    /// <summary>True while a unit tracked here is still alive in the world.</summary>
    protected bool IsAlive(Info unit)
        => unit != null && !unit.Destroyed && unit.Machine != null;

    public override void OnStart()
    {
        base.OnStart();
        if (Save.Inst == null) return; // not in a world yet — OnUpdate restores once it exists
        _seenDay = Save.Inst.day;
        RestorePopulation(); // the structure just (re)loaded — bring its units back
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        // No timers — just refill once at the start of each new day.
        if (Save.Inst == null || Save.Inst.day == _seenDay) return;
        _seenDay = Save.Inst.day;
        RestorePopulation();
    }

    /// <summary>Spawns one unit. <paramref name="index"/> runs from the current
    /// head count up to <see cref="MaxUnits"/>, so multi-unit spawners can spread
    /// spawns out across the available slots. Return the spawned Info, or null if
    /// nothing was (or could be) spawned.</summary>
    protected abstract Info SpawnUnit(int index);

    /// <summary>Drops dead units and spawns just enough to reach MaxUnits. Safe to
    /// call at any time — it only ever tops up the shortfall.</summary>
    private void RestorePopulation()
    {
        _units.RemoveAll(u => !IsAlive(u));
        for (int i = _units.Count; i < MaxUnits; i++)
        {
            Info unit = SpawnUnit(i);
            if (unit != null)
                _units.Add(unit);
        }
    }
}
