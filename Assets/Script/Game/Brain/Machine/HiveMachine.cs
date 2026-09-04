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

    /// <summary>Rings the spawns around the hive so the swarm fans out instead of stacking.</summary>
    protected override Info SpawnUnit(int index)
    {
        Vector3Int basePos = Vector3Int.FloorToInt(transform.position);
        float angle = index * (Mathf.PI * 2f / MaxUnits);
        Vector3Int spawnPos = basePos + new Vector3Int(
            Mathf.RoundToInt(Mathf.Cos(angle) * 2f), 2, Mathf.RoundToInt(Mathf.Sin(angle) * 2f));
        return Entity.Spawn(ID.Hornet, spawnPos);
    }
}
