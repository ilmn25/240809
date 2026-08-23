using UnityEngine;

public class GenAbyss : Gen
{
    public override Vector3Int GetSize() => new Vector3Int(25, 4, 25);
    public override Vector3Int GetSpawnPoint() => new Vector3Int(GetSize().x / 2, GetSize().y - 2, GetSize().z / 2) * World.ChunkSize;
    
    protected override void GenChunk(Vector3Int currentCoordinate, Chunk currentChunk)
    {
        // Procedural order: base land → biome surfaces → mountains → void edge → ravines → features.
        GenTaskStone.Run(currentCoordinate, currentChunk);
        GenTaskGranite.Run(currentCoordinate, currentChunk);  
        GenTaskMarble.Run(currentCoordinate, currentChunk);
        GenTaskDirt.Run(currentCoordinate, currentChunk);
        GenTaskSand.Run(currentCoordinate, currentChunk);
        GenTaskForest.Run(currentCoordinate, currentChunk);
        GenTaskMountain.Run(currentCoordinate, currentChunk);
        GenTaskVoid.Run(currentCoordinate, currentChunk); // void replaced by plateau
        // GenTaskRavine.Run(currentCoordinate, currentChunk); // temporarily disabled — replaced by plateaus
        // GenTaskPlateau.Run(currentCoordinate, currentChunk);
        // GenTaskWall.Run(currentCoordinate, currentChunk);
        GenTaskCaves.Run(currentCoordinate, currentChunk);
    }

    // Owl statue beside spawn respawns the Guide after the chunks are generated.
    protected override void GenPostWorld(World world)
    {
        GenTaskSpawnStatue.Run(world);
    }
}
