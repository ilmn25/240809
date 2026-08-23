using UnityEngine;

/// <summary>Shared helpers for the once-per-world scatter gen tasks (graveyard,
/// raider camp, ponds): grass-surface picking, surface lookup, and static-entity
/// placement into chunks.</summary>
public abstract class GenTaskScatter : IGenTask
{
    /// <summary>Places this world's scatter content.</summary>
    public abstract void RunWorld(World world);

    /// <summary>Random grass-biome column with a surface, or (-1,0,0) if none found.</summary>
    protected static Vector3Int PickGrassCenter(World world, System.Random rng)
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

    /// <summary>Random surface position within <paramref name="radius"/> of a center, or (-1,0,0).</summary>
    protected static Vector3Int ScatterAround(World world, Vector3Int center, System.Random rng, int radius)
    {
        for (int attempt = 0; attempt < 6; attempt++)
        {
            int x = center.x + rng.Next(-radius, radius + 1);
            int z = center.z + rng.Next(-radius, radius + 1);
            int surfaceY = FindSurfaceY(world, x, z);
            if (surfaceY < 0) continue;
            return new Vector3Int(x, surfaceY, z);
        }
        return new Vector3Int(-1, 0, 0);
    }

    /// <summary>Adds a static entity to the chunk containing <paramref name="cell"/>.</summary>
    protected static void PlaceEntity(World world, Vector3Int cell, ID id)
    {
        if (id == ID.Null) return;
        PlaceInfo(world, cell, Entity.CreateInfo(id, cell));
    }

    /// <summary>Adds an already-created info (e.g. a filled chest) to its chunk.</summary>
    protected static void PlaceInfo(World world, Vector3Int cell, Info info)
    {
        if (info == null) return;
        Chunk chunk = world[World.GetChunkCoordinate(cell)];
        if (chunk == null || chunk == Chunk.Zero) return;
        chunk.StaticEntity.Add(info);
    }

    /// <summary>First air block directly above a solid block (the ground surface). Returns -1 if none.</summary>
    protected static int FindSurfaceY(World world, int x, int z)
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
