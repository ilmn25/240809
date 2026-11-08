using System.Collections.Generic;
using UnityEngine;

/// <summary>World-gen corpses: a remains entity with starter loot scattered on the ground
/// beside it. New corpse types are added to <see cref="Definitions"/> so every generator
/// shares the same scatter rules.</summary>
public static class GenCorpse
{
    public enum Kind
    {
        Skeleton,
        // Future corpse types go here, each with an entry in Definitions.
    }

    private const int LootRadius = 1; // how far loot may land from the body

    private readonly struct Definition
    {
        public readonly ID Entity;
        public readonly int MinLoot;
        public readonly int MaxLoot;
        public readonly System.Func<System.Random, ID> PickLoot;

        public Definition(ID entity, int minLoot, int maxLoot, System.Func<System.Random, ID> pickLoot)
        {
            Entity = entity;
            MinLoot = minLoot;
            MaxLoot = maxLoot;
            PickLoot = pickLoot;
        }
    }

    private static readonly Dictionary<Kind, Definition> Definitions =
        new Dictionary<Kind, Definition>
        {
            { Kind.Skeleton, new Definition(ID.Skeleton, 1, 2, PickSkeletonLoot) },
        };

    /// <summary>Places a <paramref name="kind"/> corpse at <paramref name="cell"/> with its
    /// starter loot dropped on the ground beside it.</summary>
    /// <param name="isFree">Optional filter for the loot cells, e.g. to keep drops off
    /// occupied cells. Null allows any cell.</param>
    public static void Spawn(World world, Vector3Int cell, System.Random rng, Kind kind,
        System.Func<Vector3Int, bool> isFree = null)
    {
        Definition definition = Definitions[kind];
        GenBlocks.PlaceEntity(world, cell, definition.Entity);

        int count = rng.Next(definition.MinLoot, definition.MaxLoot + 1);
        for (int i = 0; i < count; i++)
        {
            int x = cell.x + rng.Next(-LootRadius, LootRadius + 1);
            int z = cell.z + rng.Next(-LootRadius, LootRadius + 1);
            if (x < 1 || z < 1 || x >= world.Bounds.x - 1 || z >= world.Bounds.z - 1) continue;

            Vector3Int lootCell = new Vector3Int(x, cell.y, z);
            if (lootCell == cell) continue;
            if (isFree != null && !isFree(lootCell)) continue;

            // Loose items go to DynamicEntity, so they spawn as real ground drops.
            GenBlocks.PlaceEntity(world, lootCell, definition.PickLoot(rng));
        }
    }

    /// <summary>A skeleton remains with low-tier starter loot scattered beside it — the
    /// "someone died here" scatter shared by surface generation and the spawn garden.</summary>
    public static void SpawnSkeleton(World world, Vector3Int cell, System.Random rng,
        System.Func<Vector3Int, bool> isFree = null)
        => Spawn(world, cell, rng, Kind.Skeleton, isFree);

    /// <summary>Low-tier starter loot found beside a skeleton.</summary>
    private static ID PickSkeletonLoot(System.Random rng)
    {
        double roll = rng.NextDouble();
        double chance = 0;
        if ((chance += 0.25) > roll) return ID.CrudeHatchet;
        if ((chance += 0.25) > roll) return ID.CrudePickaxe;
        if ((chance += 0.25) > roll) return ID.Torch;
        return ID.Spear;
    }
}
