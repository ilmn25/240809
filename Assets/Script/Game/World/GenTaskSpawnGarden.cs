using UnityEngine;

/// <summary>Decorates the ground around the world spawn with grass, wild flowers and starter
/// harvestables (fallen trees for wood, mud piles for mud), so a freshly spawned player has
/// gathering material within a few steps of where they start. Density fades smoothly outwards
/// over the outer few blocks instead of ending at a hard edge, and a few old skeletons with
/// loot dropped beside them ring the faded rim. Runs once per world, after chunk generation,
/// before <see cref="GenTaskEntity"/>.</summary>
public class GenTaskSpawnGarden : GenTaskScatter
{
    /// <summary>Radius (blocks) around spawn where the garden has faded out entirely.</summary>
    public const int GardenRadius = 8;

    /// <summary>Corpses sit out in the garden's fading rim, never on top of the spawn point.</summary>
    private const int CorpseMinRadius = 5;
    private const int CorpseMaxRadius = GardenRadius;
    private const int MinCorpses = 3;
    private const int MaxCorpses = 4;

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

    /// <summary>Spreads the corpses evenly around the garden's fading rim, with a little wobble
    /// so the ring doesn't look mechanical.</summary>
    private static void PlaceCorpses(World world, System.Random rng, Vector3Int spawn)
    {
        int count = rng.Next(MinCorpses, MaxCorpses + 1);
        for (int i = 0; i < count; i++)
        {
            float baseAngle = (float)i / count * Mathf.PI * 2f;
            for (int attempt = 0; attempt < 8; attempt++)
            {
                float angle = baseAngle + (float)(rng.NextDouble() - 0.5) * 0.8f;
                float radius = Mathf.Lerp(CorpseMinRadius, CorpseMaxRadius, (float)rng.NextDouble());
                int x = spawn.x + Mathf.RoundToInt(Mathf.Cos(angle) * radius);
                int z = spawn.z + Mathf.RoundToInt(Mathf.Sin(angle) * radius);
                if (!TrySurfaceCell(world, x, z, out Vector3Int cell)) continue;
                if (IsOccupied(world, cell)) continue;

                SpawnSkeletonCorpse(world, cell, rng, lootCell => !IsOccupied(world, lootCell));
                break;
            }
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
