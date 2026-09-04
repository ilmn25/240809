using UnityEngine;

/// <summary>A tent that keeps one hireable mercenary beside it. NPCs aren't saved,
/// so a fresh mercenary turns up each new day (or when the tent loads) whenever the
/// current one dies.</summary>
public class MercenaryTentMachine : SpawnerStructureMachine
{
    public static Info CreateInfo()
    {
        return new StructureInfo
        {
            Health = 120,
            Loot = ID.MercenaryTent,
            SfxHit = SfxID.HitStone,
            SfxDestroy = SfxID.HitStone,
            operationType = OperationType.Cutting,
            threshold = 1,
            SpawnsRubble = false,
        };
    }

    /// <summary>Spawns a fresh hireable mercenary beside the tent.</summary>
    protected override Info SpawnUnit(int index)
    {
        Vector3Int spawnPos = Vector3Int.FloorToInt(transform.position) + new Vector3Int(1, 2, 0);
        Info mercInfo = Entity.Spawn(ID.Mercenary, spawnPos);
        if (mercInfo?.Machine is MercenaryMachine merc)
            merc.Tent = this;
        return mercInfo;
    }

    public void Consume()
    {
        if (Info.Destroyed) return;
        Info.Destroy();
        Unload();
    }
}
