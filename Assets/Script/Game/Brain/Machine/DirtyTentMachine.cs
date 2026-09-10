using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>A dirty tent that keeps a small band of raiders, scouts and guards
/// around it. Mobs aren't saved, so the tent re-mans the camp each new day
/// (or when the tent loads). Every band member is leashed to the tent, so it
/// returns home once dragged too far from camp.</summary>
public class DirtyTentMachine : SpawnerStructureMachine
{
    protected override int MaxUnits => 4;

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

    /// <summary>Spawns one band member in the tent's own cell; it walks free on
    /// its own and stays leashed to the tent.</summary>
    protected override Info SpawnUnit(int index)
    {
        ID mobID = Random.value < 0.5f ? ID.RaiderGuard : (Random.value < 0.5f ? ID.Raider : ID.Chito);
        Info mobInfo = Entity.Spawn(mobID, Vector3Int.FloorToInt(transform.position));
        GuardModule.Attach(mobInfo, transform.position);
        return mobInfo;
    }
}