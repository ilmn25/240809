using UnityEngine;

public class GenAbyss : Gen
{
    public override Vector3Int GetSize() => new Vector3Int(60, 7, 60);
    public override Vector3Int GetSpawnPoint() => new Vector3Int(GetSize().x / 2, GetSize().y - 2, GetSize().z / 2) * World.ChunkSize;
    
    protected override void GenChunk(Vector3Int currentCoordinate, Chunk currentChunk)
    { 
        GenTaskStone.Run(currentCoordinate, currentChunk);
        GenTaskGranite.Run(currentCoordinate, currentChunk);  
        GenTaskMarble.Run(currentCoordinate, currentChunk);
        GenTaskDirt.Run(currentCoordinate, currentChunk);
        GenTaskSand.Run(currentCoordinate, currentChunk);
        // GenTaskCrater.Run(currentCoordinate, currentChunk);
        GenTaskCaves.Run(currentCoordinate, currentChunk);
        // GenTaskHouse.Run(currentCoordinate, currentChunk);
        // GenTaskThrone.Run(currentCoordinate, currentChunk);
        GenTaskWall.Run(currentCoordinate, currentChunk);
        GenTaskEntity.Run(currentCoordinate, currentChunk);
    }
    
}
