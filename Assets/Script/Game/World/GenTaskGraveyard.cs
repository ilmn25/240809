using UnityEngine;

/// <summary>Places a single graveyard cluster in the grass biome: a few headstones
/// scattered around a deterministic center point. Each headstone sits over a dug
/// grave pit holding a buried skeleton with loot stacked above it. Runs once per
/// world, after chunk generation (like the spawn owl statue).</summary>
public class GenTaskGraveyard : GenTaskScatter
{
    private const int MinHeadstones = 7;
    private const int MaxHeadstones = 10;
    private const int ScatterRadius = 5;

    /// <summary>Places the graveyard, if any, for this world.</summary>
    public override void RunWorld(World world)
    {
        System.Random rng = new System.Random((int)Gen.GetDeterministicOffset("Graveyard"));

        Vector3Int center = PickGrassCenter(world, rng);
        if (center.x < 0) return;

        int count = rng.Next(MinHeadstones, MaxHeadstones + 1);
        for (int i = 0; i < count; i++)
        {
            Vector3Int spot = ScatterAround(world, center, rng, ScatterRadius);
            if (spot.x < 0) continue;
            PlaceGrave(world, spot, rng);
        }
    }

    /// <summary>Places a headstone on top of a solid grass block, with a skeleton
    /// buried beneath that block and loot in an empty air block to the side.</summary>
    private static void PlaceGrave(World world, Vector3Int surface, System.Random rng)
    {
        Chunk chunk = world[World.GetChunkCoordinate(surface)];
        if (chunk == null || chunk == Chunk.Zero) return;

        // Headstone sits on the surface.
        chunk.StaticEntity.Add(Entity.CreateInfo(ID.Headstone, surface));

        // Skeleton and loot buried beneath the headstone (loot stacked above).
        SetAir(world, surface + new Vector3Int(0, -2, 0));
        SetAir(world, surface + new Vector3Int(0, -3, 0));
        PlaceEntity(world, surface + new Vector3Int(0, -3, 0), ID.Skeleton);
        PlaceEntity(world, surface + new Vector3Int(0, -2, 0), PickGraveLoot(rng));
    }

    /// <summary>Clears the block at <paramref name="cell"/> to air.</summary>
    private static void SetAir(World world, Vector3Int cell)
    {
        if (!World.IsInWorldBounds(cell)) return;

        Vector3Int chunkCoord = World.GetChunkCoordinate(cell);
        Chunk chunk = world[chunkCoord];
        if (chunk == null || chunk == Chunk.Zero) return;

        int localY = cell.y - chunkCoord.y;
        if (localY <= 0) return;
        chunk[cell.x - chunkCoord.x, localY, cell.z - chunkCoord.z] = 0;
    }

    private static ID PickGraveLoot(System.Random rng)
    {
        double roll = rng.NextDouble();
        double chance = 0;
        if ((chance += 0.25) > roll) return ID.CrudeHatchet;
        if ((chance += 0.25) > roll) return ID.CrudePickaxe;
        if ((chance += 0.2) > roll) return ID.CrudeMallet;
        if ((chance += 0.2) > roll) return ID.Flint;
        return ID.Sticks;
    }
}
