using UnityEngine;

/// <summary>Decorates the ground around the world spawn with grass, wild flowers and starter
/// harvestables (fallen trees for wood, mud piles for mud), plus a couple of old skeletons
/// with loot dropped beside them, so a freshly spawned player has gathering material within a
/// few steps of where they start. Density fades smoothly outwards over the outer few blocks
/// instead of ending at a hard edge. Runs once per world, after chunk generation, before
/// <see cref="GenTaskEntity"/>.</summary>
public class GenTaskSpawnGarden : GenTaskScatter
{
    /// <summary>Radius (blocks) around spawn where the garden has faded out entirely.</summary>
    public const int GardenRadius = 8;

    /// <summary>Radius (blocks) around spawn that the skeletons are dropped in.</summary>
    private const int CorpseRadius = 2;
    private const int MinCorpses = 2;
    private const int MaxCorpses = 3;

    /// <summary>What can grow in a garden column. <c>Weight</c> is the relative chance right
    /// at the spawn point; <c>FadeRadius</c> is how far out that decoration thins to nothing,
    /// so the garden trails off instead of ending at a hard edge.</summary>
    private static readonly (double Weight, int FadeRadius, System.Func<System.Random, ID> Pick)[] Decor =
    {
        (45, GardenRadius, _ => ID.Grass),
        (35, GardenRadius - 1, PickFlower),
        (10, 4, _ => ID.FallenTree),
        (10, 4, _ => ID.MudPile),
    };

    /// <summary>Sows the skeletons, then decorates the free grass columns around spawn,
    /// densest at the centre and thinning outwards.</summary>
    public override void RunWorld(World world)
    {
        System.Random rng = Gen.CreateWorldRandom("SpawnGarden");
        Vector3Int spawn = world.SpawnPoint;

        // Corpses first, so the decoration pass treats their cells and their loot as taken.
        PlaceCorpses(world, rng, spawn);

        ForEachColumnAround(spawn, GardenRadius, (x, z) =>
        {
            if (!TrySurfaceCell(world, x, z, out Vector3Int cell)) return;
            if (IsOccupied(world, cell)) return;

            float distance = Vector2.Distance(new Vector2(x, z), new Vector2(spawn.x, spawn.z));
            PlaceEntity(world, cell, PickDecoration(rng, distance));
        });
    }

    private static void PlaceCorpses(World world, System.Random rng, Vector3Int spawn)
    {
        int count = rng.Next(MinCorpses, MaxCorpses + 1);
        for (int i = 0; i < count; i++)
        {
            Vector3Int cell = PickFreeSurfaceCell(world, rng, spawn, CorpseRadius);
            if (cell.x < 0) continue;
            SpawnSkeletonCorpse(world, cell, rng, lootCell => !IsOccupied(world, lootCell));
        }
    }

    /// <summary>Picks a decoration by weight, then applies its distance falloff, or
    /// <see cref="ID.Null"/> when the column stays bare.</summary>
    private static ID PickDecoration(System.Random rng, float distance)
    {
        double total = 0;
        foreach (var entry in Decor) total += entry.Weight;

        double roll = rng.NextDouble() * total;
        foreach (var entry in Decor)
        {
            if (roll < entry.Weight)
                return rng.NextDouble() < DensityAt(distance, entry.FadeRadius)
                    ? entry.Pick(rng)
                    : ID.Null;
            roll -= entry.Weight;
        }
        return ID.Null;
    }

    /// <summary>Density at <paramref name="distance"/> from spawn: 1 at the centre, easing
    /// smoothly to 0 at <paramref name="fadeRadius"/>.</summary>
    private static double DensityAt(float distance, int fadeRadius)
    {
        if (distance >= fadeRadius) return 0;
        float t = distance / fadeRadius;
        return 1 - t * t;
    }

    private static ID PickFlower(System.Random rng)
    {
        double flower = rng.NextDouble();
        if (flower < 0.34) return ID.Orchids;
        if (flower < 0.67) return ID.Tulip;
        return ID.Daisies;
    }
}
