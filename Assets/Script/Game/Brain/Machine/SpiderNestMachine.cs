using UnityEngine;

/// <summary>A spider nest that keeps a small pack of spiders around it. Spiders
/// aren't saved, so the nest re-stocks the pack each new day (or when the nest
/// loads).</summary>
public class SpiderNestMachine : SpawnerStructureMachine
{
    protected override int MaxUnits => 6;

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
            SpawnsRubble = false,
        };
    }

    /// <summary>Spawns a spider in the nest's own cell; it walks free on its own
    /// and stays leashed to the nest.</summary>
    protected override Info SpawnUnit(int index)
    {
        Info spider = Entity.Spawn(ID.Spider, Vector3Int.FloorToInt(transform.position));
        GuardModule.Attach(spider, transform.position);
        return spider;
    }
}