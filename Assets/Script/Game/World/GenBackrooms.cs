using UnityEngine;

public class GenBackrooms : Gen
{
    public override Vector3Int GetSize() => new Vector3Int(60, 1, 60);
    public override Vector3Int GetSpawnPoint() => new Vector3Int(GetSize().x / 2 * World.ChunkSize + 1, 2, GetSize().z / 2 * World.ChunkSize + 1);

    protected override void GenChunk(Vector3Int currentCoordinate, Chunk currentChunk)
    {
        GenTaskMaze.Run(currentCoordinate, currentChunk);
    }
}
