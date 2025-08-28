using UnityEngine;

public class GenAbyss : Gen
{
    public static void Run(Vector3Int currentCoordinate, Chunk currentChunk)
    { 
        if (currentCoordinate == SpawnPoint)
        {
            PlayerInfo player = (PlayerInfo) Entity.CreateInfo(ID.Player,SpawnPoint);
            player.spawnPoint = SpawnPoint;
            World.Inst[SpawnPoint].DynamicEntity.Add(player); 
            World.Inst.target.Add(player);
            
            player = (PlayerInfo) Entity.CreateInfo(ID.Player, SpawnPoint);
            player.spawnPoint = SpawnPoint;
            player.CharSprite = ID.Sheep;
            World.Inst[SpawnPoint].DynamicEntity.Add(player); 
            World.Inst.target.Add(player);
        } 
        
        GenTaskStone.Run(currentCoordinate, currentChunk);
        GenTaskGranite.Run(currentCoordinate, currentChunk);  
        GenTaskMarble.Run(currentCoordinate, currentChunk);
        GenTaskDirt.Run(currentCoordinate, currentChunk);
        GenTaskSand.Run(currentCoordinate, currentChunk);
        GenTaskMaze.Run(currentCoordinate, currentChunk);
        GenTaskCrater.Run(currentCoordinate, currentChunk);
        GenTaskCaves.Run(currentCoordinate, currentChunk);
        GenTaskHouse.Run(currentCoordinate, currentChunk);
        GenTaskThrone.Run(currentCoordinate, currentChunk);
        GenTaskEntity.Run(currentCoordinate, currentChunk);
    }
    
}
