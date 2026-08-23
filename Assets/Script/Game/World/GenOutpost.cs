using UnityEngine;

/// <summary>Places a fortified outpost watchtower (Set/Outpost.json) in the grass
/// biome. Runs once per world, after chunk generation (like the raider camp).</summary>
public class GenOutpost : GenTaskScatter
{
    private static readonly Chunk Outpost = SetPiece.LoadSetPieceFile("Outpost");

    public override void RunWorld(World world)
    {
        if (Outpost == null) return;
        System.Random rng = new System.Random((int)Gen.GetDeterministicOffset("Outpost"));
        Vector3Int column = PickGrassCenter(world, rng);
        if (column.x < 0) return;
        int surfaceY = FindSurfaceY(world, column.x, column.z);
        if (surfaceY < 0) return;
        SetPiece.Paste(world, new Vector3Int(column.x, surfaceY, column.z), Outpost);
    }
}
