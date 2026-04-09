using UnityEngine;

public class GenAbyss : Gen
{
    public GenAbyss ()
    {
        Size = new Vector3Int(60, 30, 60);
        WorldHeight = (Size.y - 3) * World.ChunkSize;
        SpawnPoint = new Vector3Int(Size.x / 2, Size.y - 2, Size.z / 2) * World.ChunkSize;
    }

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
