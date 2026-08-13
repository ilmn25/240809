using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>A dirty tent structure that spawns and maintains a small band of
/// raiders, scouts and their guards around it. Mobs aren't saved, so the tent
/// respawns them over time, like a spider nest respawning its spiders.</summary>
public class DirtyTentMachine : StructureMachine
{
    private const int CheckInterval = 200;   // frames between checks (~3.3s at 60 fps)
    private const int RespawnDelay = 1200;   // frames before a lost mob respawns (~20s)
    private const int MaxMobs = 4;

    private readonly List<GroundMobMachine> _mobs = new List<GroundMobMachine>();

    private int _timer;
    private int _respawnTimer;

    public static Info CreateInfo()
    {
        return new StructureInfo
        {
            Health = 120,
            Loot = ID.DirtyTent,
            SfxHit = SfxID.HitStone,
            SfxDestroy = SfxID.HitStone,
            operationType = OperationType.Cutting, // axe, like trees
            threshold = 1,
        };
    }

    public override void OnStart()
    {
        base.OnStart();
        _timer = Random.Range(0, CheckInterval); // stagger tents so they don't all fire at once
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        if (++_timer < CheckInterval) return;
        _timer = 0;

        // Drop dead/spawned-from-elsewhere mobs we no longer own.
        _mobs.RemoveAll(m => m == null || m.Info == null || m.Info.Destroyed);
        if (_mobs.Count >= MaxMobs) return;

        if (_respawnTimer > 0)
        {
            _respawnTimer--;
            return;
        }

        // Spawn a mob beside the tent so it drops down next to it. Guards make
        // up the bulk of the band; the rest are roaming raiders and scouts.
        Vector3Int spawnPos = Vector3Int.FloorToInt(transform.position) + new Vector3Int(1, 2, 0);
        ID mobID = Random.value < 0.5f ? ID.RaiderGuard : (Random.value < 0.5f ? ID.Raider : ID.Chito);
        Info mobInfo = Entity.Spawn(mobID, spawnPos);
        // Guards stick to this tent.
        if (mobInfo?.Machine is RaiderGuardMachine raiderGuard)
            raiderGuard.HomePosition = transform.position;
        else if (mobInfo?.Machine is ScoutGuardMachine scoutGuard)
            scoutGuard.HomePosition = transform.position;
        if (mobInfo?.Machine is GroundMobMachine mob)
            _mobs.Add(mob);
        _respawnTimer = RespawnDelay;
    }
}