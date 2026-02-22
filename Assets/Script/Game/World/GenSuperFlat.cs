using UnityEngine;

public class GenSuperFlat : Gen
{
    private static int _id;
    private static int Brick => _id == 0 ? Block.ConvertID(ID.BrickBlock) : _id;


    public GenSuperFlat ()
    {
        Size = new Vector3Int(15, 4, 15);
        WorldHeight = (Size.y - 3) * World.ChunkSize;
        SpawnPoint = new Vector3Int(Size.x / 2, Size.y - 1, Size.z / 2) * World.ChunkSize;
    }

    protected override void GenChunk(Vector3Int currentCoordinate, Chunk currentChunk)
    {
        if (currentCoordinate == SpawnPoint)
        {
            PlayerInfo player = (PlayerInfo) Entity.CreateInfo(ID.Player, SpawnPoint);
            player.SpawnPoint = SpawnPoint;
            World.Inst[SpawnPoint].DynamicEntity.Add(player); 
            World.Inst.target.Add(player);
        } 
        
        if (currentCoordinate.y == 0)
            for (int z = 0; z < World.ChunkSize; z++)
            for (int x = 0; x < World.ChunkSize; x++)
                currentChunk[x, 0, z] = Brick;
    }
}
