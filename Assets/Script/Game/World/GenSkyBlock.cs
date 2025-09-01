using UnityEngine;

public class GenSkyBlock : Gen
{
    private static readonly Chunk SkyBlock = SetPiece.LoadSetPieceFile("SkyBlock");
    public static void Run(Vector3Int currentCoordinate, Chunk currentChunk)
    {
        if (currentCoordinate == SpawnPoint)
        {
            Vector3Int spawnPoint = currentCoordinate + new Vector3Int(3, 4, 3);
            PlayerInfo player = (PlayerInfo) Entity.CreateInfo(ID.Player,spawnPoint);
            player.spawnPoint = spawnPoint;
            World.Inst[spawnPoint].DynamicEntity.Add(player); 
            World.Inst.target.Add(player);
            
            SetPiece.Paste(currentCoordinate + Vector3Int.one, SkyBlock);
        } 
    }
}
