using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>A spider nest structure that spawns and maintains a small pack of
/// spiders around it. Spiders aren't saved, so the nest respawns them over time.</summary>
public class SpiderNestMachine : StructureMachine
{
    private const int CheckInterval = 200;   // frames between checks (~3.3s at 60 fps)
    private const int RespawnDelay = 1200;   // frames before a lost spider respawns (~20s)
    private const float SpiderSearchRadius = 30f;
    private const int MaxSpiders = 3;

    private static readonly Collider[] SpiderScanBuffer = new Collider[16];

    private int _timer;
    private int _respawnTimer;
    private int _spiderCount;

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

        _spiderCount = CountSpiders();
        if (_spiderCount >= MaxSpiders) return;

        if (_respawnTimer > 0)
        {
            _respawnTimer--;
            return;
        }

        // Spawn a spider beside the nest so it drops down next to it.
        Vector3Int spawnPos = Vector3Int.FloorToInt(transform.position) + new Vector3Int(1, 2, 0);
        Entity.Spawn(ID.Spider, spawnPos);
        _respawnTimer = RespawnDelay;
    }

    private int CountSpiders()
    {
        int count = 0;
        int hits = Physics.OverlapSphereNonAlloc(transform.position, SpiderSearchRadius, SpiderScanBuffer, Main.MaskEntity);
        for (int i = 0; i < hits; i++)
        {
            if (SpiderScanBuffer[i].TryGetComponent(out SpiderMachine spider) && !spider.Info.Destroyed)
                count++;
        }
        return count;
    }
}