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

    /// <summary>Spawns a spider beside the nest so it drops down next to it.</summary>
    protected override Info SpawnUnit(int index)
    {
        Vector3Int spawnPos = Vector3Int.FloorToInt(transform.position) + new Vector3Int(1, 2, 0);
        return Entity.Spawn(ID.Spider, spawnPos);
    }
}