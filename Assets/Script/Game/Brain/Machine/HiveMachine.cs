using UnityEngine;

/// <summary>A hive that keeps a swarm of exactly <see cref="MaxUnits"/> hornets
/// around it. Hornets aren't saved, so the hive re-stocks the swarm each new day
/// (or when the hive loads).</summary>
public class HiveMachine : SpawnerStructureMachine
{
    protected override int MaxUnits => 8;

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

    /// <summary>Spawns a hornet in the hive's own cell; it flies free on its own.</summary>
    protected override Info SpawnUnit(int index)
        => Entity.Spawn(ID.Hornet, Vector3Int.FloorToInt(transform.position));
}
