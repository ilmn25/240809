using UnityEngine;

/// <summary>Places a single graveyard cluster in the grass biome: a few headstones
/// scattered around a deterministic center point. Each headstone sits over a dug
/// grave pit holding a buried skeleton with loot stacked above it. Runs once per
/// world, after chunk generation (like the spawn owl statue).</summary>
public class GenTaskGraveyard : Gen
{
    private const int MinHeadstones = 7;
    private const int MaxHeadstones = 10;
    private const int ScatterRadius = 5;
    private const int GraveDepth = 3; // blocks below the surface to dig out

    /// <summary>Places the graveyard, if any, for this world.</summary>
    public static void Run(World world)
    {
        System.Random rng = new System.Random((int)GetDeterministicOffset("Graveyard"));

        Vector3Int center = PickGrassCenter(world, rng);
        if (center.x < 0) return;

        int count = rng.Next(MinHeadstones, MaxHeadstones + 1);
        for (int i = 0; i < count; i++)
        {
            int x = center.x + rng.Next(-ScatterRadius, ScatterRadius + 1);
            int z = center.z + rng.Next(-ScatterRadius, ScatterRadius + 1);

            int surfaceY = FindSurfaceY(world, x, z);
            if (surfaceY < 0) continue;

            PlaceGrave(world, new Vector3Int(x, surfaceY, z), rng);
        }
    }

    /// <summary>Places a headstone and the buried grave beneath it (pit, skeleton,
    /// and loot) at the given surface position.</summary>
    private static void PlaceGrave(World world, Vector3Int surface, System.Random rng)
    {
        Chunk chunk = world[World.GetChunkCoordinate(surface)];
        if (chunk == null || chunk == Chunk.Zero) return;
        chunk.StaticEntity.Add(Entity.CreateInfo(ID.Headstone, surface));

        // Dig out the grave pit below the surface.
        for (int depth = 1; depth <= GraveDepth; depth++)
        {
            Vector3Int cell = new Vector3Int(surface.x, surface.y - depth, surface.z);
            CarveCell(world, cell);
        }

        // Skeleton lies at the bottom of the pit, loot stacked one block above.
        Vector3Int skeleton = new Vector3Int(surface.x, surface.y - GraveDepth, surface.z);
        PlaceEntity(world, skeleton, ID.Skeleton);
        PlaceEntity(world, skeleton + Vector3Int.up, PickGraveLoot(rng));
    }

    /// <summary>Adds a static entity to the chunk containing <paramref name="cell"/>.</summary>
    private static void PlaceEntity(World world, Vector3Int cell, ID id)
    {
        if (id == ID.Null) return;
        Chunk chunk = world[World.GetChunkCoordinate(cell)];
        if (chunk == null || chunk == Chunk.Zero) return;
        chunk.StaticEntity.Add(Entity.CreateInfo(id, cell));
    }

    /// <summary>Removes the block at <paramref name="cell"/> to carve out a hole.</summary>
    private static void CarveCell(World world, Vector3Int cell)
    {
        if (!World.IsInWorldBounds(cell)) return;

        Vector3Int chunkCoord = World.GetChunkCoordinate(cell);
        Chunk chunk = world[chunkCoord];
        if (chunk == null || chunk == Chunk.Zero) return;

        int localY = cell.y - chunkCoord.y;
        if (localY <= 0) return; // don't carve through the chunk bottom
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

    /// <summary>Finds a random grass-biome column to center the graveyard on.
    /// Returns (-1,0,0) if none found.</summary>
    private static Vector3Int PickGrassCenter(World world, System.Random rng)
    {
        for (int attempt = 0; attempt < 40; attempt++)
        {
            int x = rng.Next(2, world.Bounds.x - 2);
            int z = rng.Next(2, world.Bounds.z - 2);
            if (GenHelpBiome.GetBiomeType(x, z) != BiomeType.Grass) continue;
            if (FindSurfaceY(world, x, z) < 0) continue;
            return new Vector3Int(x, 0, z);
        }
        return new Vector3Int(-1, 0, 0);
    }

    /// <summary>First air block directly above a solid block (the ground surface).</summary>
    private static int FindSurfaceY(World world, int x, int z)
    {
        for (int y = world.Bounds.y - 1; y >= 1; y--)
        {
            Vector3Int block = new Vector3Int(x, y, z);
            Vector3Int chunkCoord = World.GetChunkCoordinate(block);
            Chunk chunk = world[chunkCoord];
            if (chunk == null || chunk == Chunk.Zero) continue;

            int localX = block.x - chunkCoord.x;
            int localY = block.y - chunkCoord.y;
            int localZ = block.z - chunkCoord.z;
            if (localY == 0) continue;

            if (chunk[localX, localY, localZ] == 0 && chunk[localX, localY - 1, localZ] != 0)
                return y;
        }
        return -1;
    }
}
