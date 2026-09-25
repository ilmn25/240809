using UnityEngine;

public abstract class GenTaskScatter : IGenTask
{
    /// <summary>Radius (blocks) around the world spawn — the centre of the map — that is
    /// kept free of scattered structures, so raider camps, outposts, towers and the like
    /// never crowd the player's starting area. A little larger than one logic ring
    /// (<see cref="Scene.LogicDistance"/> = 45).</summary>
    public const int SpawnClearRadius = 48;

    public abstract void RunWorld(World world);

    /// <summary>True when the column (<paramref name="x"/>, <paramref name="z"/>), together
    /// with <paramref name="margin"/> blocks of its footprint, lies outside the spawn
    /// clearing around <paramref name="spawn"/>.</summary>
    public static bool IsClearOfSpawn(Vector3Int spawn, int x, int z, int margin = 0)
    {
        int dx = x - spawn.x;
        int dz = z - spawn.z;
        int radius = SpawnClearRadius + margin;
        return dx * dx + dz * dz >= radius * radius;
    }

    /// <summary>Rolls up to <paramref name="attempts"/> random grass columns that have a
    /// surface and clear the spawn clearing by <paramref name="spawnMargin"/> blocks,
    /// returning the first one that also satisfies <paramref name="accept"/> (y is unused),
    /// or (-1, 0, 0) when none does.</summary>
    protected static Vector3Int PickGrassColumn(World world, System.Random rng, int attempts,
        int spawnMargin = 0, System.Func<int, int, bool> accept = null)
    {
        for (int attempt = 0; attempt < attempts; attempt++)
        {
            int x = rng.Next(2, world.Bounds.x - 2);
            int z = rng.Next(2, world.Bounds.z - 2);
            if (!IsClearOfSpawn(world.SpawnPoint, x, z, spawnMargin)) continue;
            if (GenHelpBiome.GetBiomeType(x, z) != BiomeType.Grass) continue;
            if (FindSurfaceY(world, x, z) < 0) continue;
            if (accept != null && !accept(x, z)) continue;
            return new Vector3Int(x, 0, z);
        }
        return new Vector3Int(-1, 0, 0);
    }

    /// <summary>A valid grass column centre, or (-1, 0, 0).</summary>
    protected static Vector3Int PickGrassCenter(World world, System.Random rng, int spawnMargin = 0)
        => PickGrassColumn(world, rng, 40, spawnMargin);

    /// <summary>The origin of a flat <paramref name="size"/>-square grass footprint, snapped
    /// down to its surface, or (-1, 0, 0).</summary>
    protected static Vector3Int PickFootprintOrigin(World world, System.Random rng, int size, int maxSpread = 2, int maxAttempts = 30, int spawnMargin = 0)
    {
        Vector3Int column = PickGrassColumn(world, rng, maxAttempts, spawnMargin,
            (x, z) => FindFootprintSurface(world, x, z, size, maxSpread) >= 0);
        if (column.x < 0) return column;
        return new Vector3Int(column.x, FindSurfaceY(world, column.x, column.z), column.z);
    }

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

    /// <summary>Adds a structure to the chunk holding <paramref name="cell"/>. Scatter output
    /// is always static, unlike <see cref="GenBlocks.PlaceEntity"/> which routes loose items
    /// to DynamicEntity.</summary>
    protected static void PlaceEntity(World world, Vector3Int cell, ID id)
    {
        if (id == ID.Null) return;
        Info info = Entity.CreateInfo(id, cell);
        if (info == null) return;
        GenBlocks.ChunkAt(world, cell)?.StaticEntity.Add(info);
    }

    protected static int FindSurfaceY(World world, int x, int z)
        => GenBlocks.FindSurfaceY(world, x, z);

    protected static int FindFootprintSurface(World world, int originX, int originZ, int size, int maxSpread = 2)
    {
        int minY = int.MaxValue;
        int maxY = -1;
        for (int dx = 0; dx < size; dx++)
            for (int dz = 0; dz < size; dz++)
            {
                int y = FindSurfaceY(world, originX + dx, originZ + dz);
                if (y < 0) return -1;
                if (y < minY) minY = y;
                if (y > maxY) maxY = y;
            }
        return maxY - minY <= maxSpread ? minY : -1;
    }
}
