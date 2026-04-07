using UnityEngine;

public class GenSkyBlock : Gen
{
    private static readonly Chunk SkyBlock = SetPiece.LoadSetPieceFile("SkyBlock");


    public GenSkyBlock ()
    {
        Size = Vector3Int.one;
        WorldHeight = (Size.y - 3) * World.ChunkSize;
        SpawnPoint = Vector3Int.zero;
    }

    protected override void GenChunk(Vector3Int currentCoordinate, Chunk currentChunk)
    {
        if (currentCoordinate == SpawnPoint)
        {
            Vector3Int spawnPoint = currentCoordinate + new Vector3Int(3, 4, 3);
            SaveData.Inst.spawnPosition = spawnPoint;
            
            SetPiece.Paste(currentCoordinate + Vector3Int.one, SkyBlock);
        } 
    }
}
