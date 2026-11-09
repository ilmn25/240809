using UnityEngine;

public class GenAbyss : Gen
{
    public override Vector3Int GetSize() => new Vector3Int(25, 4, 25);
    public override Vector3Int GetSpawnPoint() => new Vector3Int(GetSize().x / 2, GetSize().y - 2, GetSize().z / 2) * World.ChunkSize;

    protected override IGenTask[] Tasks => new IGenTask[]
    {
        // Chunk pass: base land → biome surfaces → mountains → void edge → caves.
        new GenTaskStone(), new GenTaskGranite(), new GenTaskMarble(), new GenTaskDirt(),
        new GenTaskSand(), new GenTaskForest(), new GenTaskMountain(), new GenTaskVoid(),
        // new GenTaskCaves(),
        // World pass: flatten the spawn pad, entrances, spawn statue + garden, surface
        // scatter, entities last.
        new GenTaskSpawnFlat(),
        new GenDungeonEntrance(), new GenMawEntrance(), new GenTaskSpawnStatue(),
        new GenTaskSpawnGarden(),
        new GenTaskGraveyard(),
        new GenTaskBeehiveArea(), new GenTaskScavCamp(), new GenTaskPond(),
        new GenOutpost(), new GenTaskTower(), new GenTaskEntity(),
    };
}
