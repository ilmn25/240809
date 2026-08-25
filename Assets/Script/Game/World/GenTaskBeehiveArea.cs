using UnityEngine;

/// <summary>Places a single beehive meadow in the grass biome: a dense patch of
/// meadow flowers with several beehives (each guarded by hornets) scattered
/// through it. Runs once per world, after chunk generation, like the graveyard
/// cluster.</summary>
public class GenTaskBeehiveArea : GenTaskScatter
{
    private const int MinHives = 5;
    private const int MaxHives = 8;
    private const int MinFlowers = 30;
    private const int MaxFlowers = 45;
    private const int ScatterRadius = 6;

    /// <summary>Places the beehive meadow, if any, for this world.</summary>
    public override void RunWorld(World world)
    {
        System.Random rng = Gen.CreateWorldRandom("BeehiveArea");

        Vector3Int center = PickGrassCenter(world, rng);
        if (center.x < 0) return;

        var occupied = new System.Collections.Generic.HashSet<Vector3Int>();

        int hives = rng.Next(MinHives, MaxHives + 1);
        for (int i = 0; i < hives; i++)
        {
            Vector3Int spot = ScatterAround(world, center, rng, ScatterRadius);
            if (spot.x < 0) continue;
            PlaceEntity(world, spot, ID.Hive);
            occupied.Add(spot);
        }

        int flowers = rng.Next(MinFlowers, MaxFlowers + 1);
        for (int i = 0; i < flowers; i++)
        {
            Vector3Int spot = ScatterAround(world, center, rng, ScatterRadius);
            if (spot.x < 0) continue;
            if (!occupied.Add(spot)) continue;
            PlaceEntity(world, spot, PickMeadowFlower(rng));
        }
    }

    private static ID PickMeadowFlower(System.Random rng)
    {
        double flower = rng.NextDouble();
        if (flower < 0.34) return ID.Orchids;
        if (flower < 0.67) return ID.Tulip;
        return ID.Daisies;
    }
}
