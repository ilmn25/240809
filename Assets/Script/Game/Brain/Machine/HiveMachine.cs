using UnityEngine;

public class HiveMachine : SpawnerStructureMachine
{
    protected override int MaxUnits => 2;

    public static Info CreateInfo() => CreateSpawnerInfo(80, ID.Hive);

    protected override Info SpawnUnit(int index)
        => Entity.Spawn(ID.Hornet, Vector3Int.FloorToInt(transform.position));
}
