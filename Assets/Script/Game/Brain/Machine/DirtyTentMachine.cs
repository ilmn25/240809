using UnityEngine;
using Random = UnityEngine.Random;

public class DirtyTentMachine : SpawnerStructureMachine
{
    protected override int MaxUnits => 1;

    private const float GuardLeash = 2f;

    public static Info CreateInfo() => CreateSpawnerInfo(120, ID.DirtyTent);

    protected override Info SpawnUnit(int index)
    {
        ID mobID = Random.value < 0.5f ? ID.RaiderGuard : ID.ScoutGuard;
        Info mobInfo = Entity.Spawn(mobID, Vector3Int.FloorToInt(transform.position));
        GuardModule.Attach(mobInfo, transform.position, GuardLeash);
        return mobInfo;
    }
}