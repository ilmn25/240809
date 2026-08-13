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

    /// <summary>Places a headstone on top of a solid grass block, with a skeleton
    /// buried beneath that block and loot in an empty air block to the side.</summary>
    private static void PlaceGrave(World world, Vector3Int surface, System.Random rng)
    {
        Chunk chunk = world[World.GetChunkCoordinate(surface)];
        if (chunk == null || chunk == Chunk.Zero) return;

        // Headstone sits on the surface.
        chunk.StaticEntity.Add(Entity.CreateInfo(ID.Headstone, surface));

        // Solid grass block directly below the headstone.
        SetBlock(world, surface + Vector3Int.down, ID.GrassBlock);

        // Skeleton and loot in empty air blocks to the side (at surface level).
        PlaceEntity(world, surface + new Vector3Int(1, 0, 0), ID.Skeleton);
        PlaceEntity(world, surface + new Vector3Int(2, 0, 0), PickGraveLoot(rng));
    }

    /// <summary>Sets a solid block at <paramref name="cell"/>.</summary>
    private static void SetBlock(World world, Vector3Int cell, ID id)
    {
        if (!World.IsInWorldBounds(cell)) return;

        Vector3Int chunkCoord = World.GetChunkCoordinate(cell);
        Chunk chunk = world[chunkCoord];
        if (chunk == null || chunk == Chunk.Zero) return;

        int localY = cell.y - chunkCoord.y;
        if (localY <= 0) return;
        chunk[cell.x - chunkCoord.x, localY, cell.z - chunkCoord.z] = Block.ConvertID(id);
    }

    /// <summary>Adds a static entity to the chunk containing <paramref name="cell"/>.</summary>
    private static void PlaceEntity(World world, Vector3Int cell, ID id)
    {
        if (id == ID.Null) return;
        Chunk chunk = world[World.GetChunkCoordinate(cell)];
        if (chunk == null || chunk == Chunk.Zero) return;
        chunk.StaticEntity.Add(Entity.CreateInfo(id, cell));
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
