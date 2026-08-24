using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>A hive structure that spawns and maintains a swarm of exactly
/// <see cref="MaxHornets"/> hornets around it. Hornets aren't saved, so the hive
/// respawns them over time.</summary>
public class HiveMachine : StructureMachine
{
    private const int CheckInterval = 200;   // frames between checks (~3.3s at 60 fps)
    private const int RespawnDelay = 1200;   // frames before a lost hornet respawns (~20s)
    private const int MaxHornets = 8;

    private readonly List<HornetMachine> _hornets = new List<HornetMachine>();

    private int _timer;
    private int _respawnTimer;

    public static Info CreateInfo()
    {
        return new StructureInfo
        {
            Health = 80,
            Loot = ID.Hive,
            SfxHit = SfxID.HitStone,
            SfxDestroy = SfxID.HitStone,
            operationType = OperationType.Cutting, // axe, like the spider nest
            threshold = 1,
            SpawnsRubble = false,
        };
    }

    public override void OnStart()
    {
        base.OnStart();
        // Spawn the full swarm up front, ringed around the hive.
        for (int i = 0; i < MaxHornets; i++)
            SpawnHornet();
        _timer = Random.Range(0, CheckInterval); // stagger hives so they don't all fire at once
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        if (++_timer < CheckInterval) return;
        _timer = 0;

        // Drop dead/spawned-from-elsewhere hornets we no longer own.
        _hornets.RemoveAll(h => h == null || h.Info == null || h.Info.Destroyed);
        if (_hornets.Count >= MaxHornets) return;

        if (_respawnTimer > 0)
        {
            _respawnTimer--;
            return;
        }

        SpawnHornet();
        _respawnTimer = RespawnDelay;
    }

    private void SpawnHornet()
    {
        // Ring the spawns around the hive so the swarm fans out instead of stacking.
        Vector3Int basePos = Vector3Int.FloorToInt(transform.position);
        float angle = _hornets.Count * (Mathf.PI * 2f / MaxHornets);
        Vector3Int spawnPos = basePos + new Vector3Int(
            Mathf.RoundToInt(Mathf.Cos(angle) * 2f), 2, Mathf.RoundToInt(Mathf.Sin(angle) * 2f));

        Info hornetInfo = Entity.Spawn(ID.Hornet, spawnPos);
        if (hornetInfo?.Machine is HornetMachine hornet)
            _hornets.Add(hornet);
    }
}
