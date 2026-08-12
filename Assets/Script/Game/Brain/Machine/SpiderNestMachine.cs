using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>A spider nest structure that spawns and maintains a small pack of
/// spiders around it. Spiders aren't saved, so the nest respawns them over time.</summary>
public class SpiderNestMachine : StructureMachine
{
    private const int CheckInterval = 200;   // frames between checks (~3.3s at 60 fps)
    private const int RespawnDelay = 1200;   // frames before a lost spider respawns (~20s)
    private const int MaxSpiders = 6;

    private readonly List<SpiderMachine> _spiders = new List<SpiderMachine>();

    private int _timer;
    private int _respawnTimer;

    public static Info CreateInfo()
    {
        return new StructureInfo
        {
            Health = 120,
            Loot = ID.SpiderNest,
            SfxHit = SfxID.HitStone,
            SfxDestroy = SfxID.HitStone,
            operationType = OperationType.Cutting, // axe, like trees
            threshold = 1,
        };
    }

    public override void OnStart()
    {
        base.OnStart();
        _timer = Random.Range(0, CheckInterval); // stagger nests so they don't all fire at once
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        if (++_timer < CheckInterval) return;
        _timer = 0;

        // Drop dead/spawned-from-elsewhere spiders we no longer own.
        _spiders.RemoveAll(s => s == null || s.Info == null || s.Info.Destroyed);
        if (_spiders.Count >= MaxSpiders) return;

        if (_respawnTimer > 0)
        {
            _respawnTimer--;
            return;
        }

        // Spawn a spider beside the nest so it drops down next to it.
        Vector3Int spawnPos = Vector3Int.FloorToInt(transform.position) + new Vector3Int(1, 2, 0);
        Info spiderInfo = Entity.Spawn(ID.Spider, spawnPos);
        if (spiderInfo?.Machine is SpiderMachine spider)
            _spiders.Add(spider);
        _respawnTimer = RespawnDelay;
    }
}