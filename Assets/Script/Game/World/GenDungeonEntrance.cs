using UnityEngine;

/// <summary>Places a brick stairwell descending to an underground dungeon door
/// (Set/DungeonEntrance.json) in the grass biome. Runs once per world, after
/// chunk generation (like the outpost and raider camp).</summary>
public class GenDungeonEntrance : GenTaskScatter
{
    /// <summary>How far below the surface the stairwell floor and door sit.</summary>
    private const int DepthOffset = 5;

    private static readonly Chunk Entrance = SetPiece.LoadSetPieceFile("DungeonEntrance");

    public static void Run(World world)
    {
        if (Entrance == null) return;
        System.Random rng = new System.Random((int)GetDeterministicOffset("DungeonEntrance"));
        Vector3Int column = PickGrassCenter(world, rng);
        if (column.x < 0) return;
        int surfaceY = FindSurfaceY(world, column.x, column.z);
        if (surfaceY < 0) return;
        SetPiece.Paste(world, new Vector3Int(column.x, surfaceY - DepthOffset, column.z), Entrance);
    }
}
