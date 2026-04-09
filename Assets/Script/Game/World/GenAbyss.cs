using UnityEngine;

public class GenAbyss : Gen
{
    protected override void GenChunk(Vector3Int currentCoordinate, Chunk currentChunk)
    { 
        GenTaskStone.Run(currentCoordinate, currentChunk);
        GenTaskGranite.Run(currentCoordinate, currentChunk);  
        GenTaskMarble.Run(currentCoordinate, currentChunk);
        GenTaskDirt.Run(currentCoordinate, currentChunk);
        GenTaskSand.Run(currentCoordinate, currentChunk);
        GenTaskMaze.Run(currentCoordinate, currentChunk);
        // GenTaskCrater.Run(currentCoordinate, currentChunk);
        GenTaskCaves.Run(currentCoordinate, currentChunk);
        // GenTaskHouse.Run(currentCoordinate, currentChunk);
        // GenTaskThrone.Run(currentCoordinate, currentChunk);
        GenTaskWall.Run(currentCoordinate, currentChunk);
        GenTaskEntity.Run(currentCoordinate, currentChunk);
    }
    
}
