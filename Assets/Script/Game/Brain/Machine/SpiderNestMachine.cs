using UnityEngine;

public class SpiderNestMachine : SpawnerStructureMachine
{
    protected override int MaxUnits => 3;

    public static Info CreateInfo() => CreateSpawnerInfo(120, ID.SpiderNest);

    protected override Info SpawnUnit(int index)
    {
        Info spider = Entity.Spawn(ID.Spider, Vector3Int.FloorToInt(transform.position));
        GuardModule.Attach(spider, transform.position);
        return spider;
    }
}