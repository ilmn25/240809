using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>A scavenger tent: one lean-to that keeps a scavenger or a scavenger archer
/// leashed nearby. Scav camps — and the odd tent scattered in the wild — are built from
/// these.</summary>
public class ScavTentMachine : SpawnerStructureMachine
{
    protected override int MaxUnits => 1;
    protected override bool SpawnsHostiles => true;

    private const float GuardLeash = 2f;

    public static Info CreateInfo() => CreateSpawnerInfo(120, ID.ScavTent);

    protected override Info SpawnUnit(int index)
    {
        ID mobID = Random.value < 0.5f ? ID.Scavenger : ID.ScavengerArcher;
        Info mobInfo = Entity.Spawn(mobID, Vector3Int.FloorToInt(transform.position));
        GuardModule.Attach(mobInfo, transform.position, GuardLeash);
        return mobInfo;
    }
}