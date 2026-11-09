using UnityEngine;

/// <summary>Scatters a few small scavenger camps across the world: a stash chest ringed by
/// scav tents and a lamp. Replaces the old raider camps — the same ground, held by far
/// weaker squatters.</summary>
public class GenTaskScavCamp : GenTaskScatter
{
    private const int CampCount = 2;
    private const int MinTents = 2;
    private const int MaxTents = 3;
    private const int ClusterRadius = 4;
    /// <summary>Width of the flat square a camp needs (ClusterRadius on both sides of its centre).</summary>
    private const int CampSize = ClusterRadius * 2 + 1;

    public override void RunWorld(World world)
    {
        System.Random rng = Gen.CreateWorldRandom("ScavCamp");

        for (int i = 0; i < CampCount; i++)
        {
            Vector3Int column = FindCampCenter(world, rng);
            if (column.x < 0) continue;
            PlaceCamp(world, column, rng);
        }
    }

    /// <summary>A flat camp-sized patch of grass clear of the spawn clearing, snapped to its
    /// surface, or (-1, 0, 0).</summary>
    private static Vector3Int FindCampCenter(World world, System.Random rng)
    {
        Vector3Int column = PickGrassColumn(world, rng, 40, ClusterRadius,
            (x, z) => FindFootprintSurface(world, x - ClusterRadius, z - ClusterRadius, CampSize, maxSpread: 3) >= 0);
        if (column.x < 0) return column;
        return new Vector3Int(column.x, FindSurfaceY(world, column.x, column.z), column.z);
    }

    private static void PlaceCamp(World world, Vector3Int center, System.Random rng)
    {
        GenBlocks.PlaceEntity(world, center, ID.ScavCampChest);

        int tentCount = rng.Next(MinTents, MaxTents + 1);
        for (int i = 0; i < tentCount; i++)
        {
            Vector3Int spot = ScatterAround(world, center, rng, ClusterRadius);
            if (spot.x < 0) continue;
            GenBlocks.PlaceEntity(world, spot, ID.ScavTent);
        }

        Vector3Int lampSpot = ScatterAround(world, center, rng, ClusterRadius);
        if (lampSpot.x >= 0) GenBlocks.PlaceEntity(world, lampSpot, ID.Lamp);
    }
}