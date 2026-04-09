using UnityEngine;

public class GenSkyBlock : Gen
{
    private static readonly Chunk SkyBlock = SetPiece.LoadSetPieceFile("SkyBlock");

    public override Vector3Int GetSize() => Vector3Int.one;
    public override Vector3Int GetSpawnPoint() => new Vector3Int(3, 5, 3);

    protected override void GenChunk(Vector3Int currentCoordinate, Chunk currentChunk)
    {
        if (currentCoordinate == Vector3Int.zero)
        {
            SetPiece.Paste(currentCoordinate + Vector3Int.one, SkyBlock);
        } 
    }
}
