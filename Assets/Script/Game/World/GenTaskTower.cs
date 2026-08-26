using System.Collections.Generic;
using UnityEngine;

/// <summary>Places a few large circular brick towers (Set/Tower.json) in the grass
/// biome, partially buried below the surface (like the dungeon entrance). Runs
/// once per world, after chunk generation (like the outpost and raider camp).</summary>
public class GenTaskTower : GenTaskScatter
{
    private const int TowerCount = 3;       // how many towers to place
    private const int MinSeparation = 48;   // keep towers from overlapping each other
    private const int DepthOffset = 10;     // how far below the surface the base sits

    private static readonly Chunk Tower = SetPiece.LoadSetPieceFile("Tower");

    public override void RunWorld(World world)
    {
        if (Tower == null) return;
        System.Random rng = Gen.CreateWorldRandom("Tower");
        List<Vector3Int> placed = new List<Vector3Int>(TowerCount);

        for (int i = 0; i < TowerCount; i++)
        {
            for (int attempt = 0; attempt < 40; attempt++)
            {
                Vector3Int column = PickGrassCenter(world, rng);
                if (column.x < 0) break;
                int surfaceY = FindSurfaceY(world, column.x, column.z);
                if (surfaceY < 0) continue;
                if (!IsClearOf(column, placed)) continue;
                placed.Add(column);
                SetPiece.Paste(world, new Vector3Int(column.x, surfaceY - DepthOffset, column.z), Tower);
                break;
            }
        }
    }

    /// <summary>True when no already-placed tower center is within MinSeparation.</summary>
    private static bool IsClearOf(Vector3Int p, List<Vector3Int> placed)
    {
        foreach (Vector3Int q in placed)
        {
            int dx = p.x - q.x;
            int dz = p.z - q.z;
            if (dx * dx + dz * dz < MinSeparation * MinSeparation) return false;
        }
        return true;
    }
}
