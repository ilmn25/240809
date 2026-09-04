using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>A dirty tent that keeps a small band of raiders, scouts and their
/// guards around it. Mobs aren't saved, so the tent re-mans the camp each new day
/// (or the next time the tent loads) until its band is back up to strength.</summary>
public class DirtyTentMachine : SpawnerStructureMachine
{
    protected override int MaxUnits => 4;

    // Ring spawn points so a freshly restored band doesn't stack on one cell.
    private static readonly Vector3Int[] SpawnPoints =
    {
        new Vector3Int(1, 2, 0),
        new Vector3Int(-1, 2, 0),
        new Vector3Int(0, 2, 1),
        new Vector3Int(0, 2, -1),
    };

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
            SpawnsRubble = false,
        };
    }

    /// <summary>Spawns one band member at the next ring slot. Guards make up the
    /// bulk of the band; the rest are roaming raiders and scouts.</summary>
    protected override Info SpawnUnit(int index)
    {
        Vector3Int spawnPos = Vector3Int.FloorToInt(transform.position) + SpawnPoints[index % SpawnPoints.Length];
        ID mobID = Random.value < 0.5f ? ID.RaiderGuard : (Random.value < 0.5f ? ID.Raider : ID.Chito);
        Info mobInfo = Entity.Spawn(mobID, spawnPos);
        // Guards stick to this tent.
        if (mobInfo?.Machine is RaiderGuardMachine raiderGuard)
            raiderGuard.HomePosition = transform.position;
        else if (mobInfo?.Machine is ScoutGuardMachine scoutGuard)
            scoutGuard.HomePosition = transform.position;
        return mobInfo;
    }
}