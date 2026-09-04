using UnityEngine;

public class GenMawEntrance : GenTaskScatter
{
    private static readonly Chunk Entrance = SetPiece.LoadSetPieceFile("MawEntrance");

    public override void RunWorld(World world)
    {
        if (Entrance == null) return;
        System.Random rng = Gen.CreateWorldRandom("MawEntrance");

        Vector3Int origin = PickFootprintOrigin(world, rng, Entrance.size);
        if (origin.x < 0) return;

        SetPiece.Paste(world, origin, Entrance);
    }
}
