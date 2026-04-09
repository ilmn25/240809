using UnityEngine;

public class GenSuperFlat : Gen
{
    private static int _id;
    private static int Brick => _id == 0 ? Block.ConvertID(ID.BrickBlock) : _id;

    public override Vector3Int GetSize() => new Vector3Int(15, 4, 15);
    public override Vector3Int GetSpawnPoint() => new Vector3Int(GetSize().x / 2, GetSize().y - 1, GetSize().z / 2) * World.ChunkSize;

    protected override void GenChunk(Vector3Int currentCoordinate, Chunk currentChunk)
    {
        if (currentCoordinate.y == 0)
            for (int z = 0; z < World.ChunkSize; z++)
            for (int x = 0; x < World.ChunkSize; x++)
                currentChunk[x, 0, z] = Brick;
    }
}
