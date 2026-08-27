using UnityEngine;

/// <summary>Places the Maw's entrance (Set/MawEntrance.json) in the grass biome
/// — a brick archway holding a MawDoor that leads to the extraction facility.
/// Runs once per world, after chunk generation (like the dungeon entrance).</summary>
public class GenMawEntrance : GenTaskScatter
{
    private static readonly Chunk Entrance = SetPiece.LoadSetPieceFile("MawEntrance");

    public override void RunWorld(World world)
    {
        if (Entrance == null) return;
        System.Random rng = Gen.CreateWorldRandom("MawEntrance");
        Vector3Int column = PickGrassCenter(world, rng);
        if (column.x < 0) return;
        int surfaceY = FindSurfaceY(world, column.x, column.z);
        if (surfaceY < 0) return;
        SetPiece.Paste(world, new Vector3Int(column.x, surfaceY, column.z), Entrance);
    }
}
