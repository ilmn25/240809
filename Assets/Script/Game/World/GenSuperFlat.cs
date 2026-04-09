using UnityEngine;

public class GenSuperFlat : Gen
{
    private static int _id;
    private static int Brick => _id == 0 ? Block.ConvertID(ID.BrickBlock) : _id;

    protected override void GenChunk(Vector3Int currentCoordinate, Chunk currentChunk)
    {
        if (currentCoordinate.y == 0)
            for (int z = 0; z < World.ChunkSize; z++)
            for (int x = 0; x < World.ChunkSize; x++)
                currentChunk[x, 0, z] = Brick;
    }
}
