using UnityEngine;

public class GenSkyBlock : Gen
{
    private static readonly Chunk SkyBlock = SetPiece.LoadSetPieceFile("SkyBlock");

    protected override void GenChunk(Vector3Int currentCoordinate, Chunk currentChunk)
    {
        if (currentCoordinate == Vector3Int.zero)
        {
            Vector3Int spawnPoint = currentCoordinate + new Vector3Int(3, 4, 3);
            World.Inst.SpawnPoint = spawnPoint;
            
            SetPiece.Paste(currentCoordinate + Vector3Int.one, SkyBlock);
        } 
    }
}
