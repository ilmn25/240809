using System.Collections.Generic;
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

    // ---- Cell queries ----

    /// <summary>Surface cell (the first air block above the ground) of a grass column, or
    /// false when the column is out of bounds, isn't grass, or has no surface.</summary>
    protected static bool TrySurfaceCell(World world, int x, int z, out Vector3Int cell)
    {
        cell = default;
        if (x < 1 || z < 1 || x >= world.Bounds.x - 1 || z >= world.Bounds.z - 1) return false;
        if (GenHelpBiome.GetBiomeType(x, z) != BiomeType.Grass) return false;

        int surfaceY = FindSurfaceY(world, x, z);
        if (surfaceY < 0) return false;

        cell = new Vector3Int(x, surfaceY, z);
        return true;
    }

    /// <summary>True when a static entity or a loose item already fills
    /// <paramref name="cell"/> — placed by an earlier gen task, or by an earlier pass of the
    /// same task — so new scatter must not stack on top of it.</summary>
    protected static bool IsOccupied(World world, Vector3Int cell)
    {
        Chunk chunk = GenBlocks.ChunkAt(world, cell);
        if (chunk == null) return true;
        return Contains(chunk.StaticEntity, cell) || Contains(chunk.DynamicEntity, cell);
    }

    private static bool Contains(List<Info> entities, Vector3Int cell)
    {
        foreach (Info entity in entities)
            if (Vector3Int.FloorToInt(entity.position) == cell)
                return true;
        return false;
    }

    /// <summary>Calls <paramref name="action"/> for every column inside the disc of
    /// <paramref name="radius"/> around <paramref name="center"/>.</summary>
    protected static void ForEachColumnAround(Vector3Int center, int radius, System.Action<int, int> action)
    {
        int radiusSquared = radius * radius;
        for (int dx = -radius; dx <= radius; dx++)
            for (int dz = -radius; dz <= radius; dz++)
            {
                if (dx * dx + dz * dz > radiusSquared) continue;
                action(center.x + dx, center.z + dz);
            }
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
            if (!TrySurfaceCell(world, x, z, out _)) continue;
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

    /// <summary>Places a skeleton remains at <paramref name="cell"/> with starter loot (crude
    /// tools, flint) dropped on the ground beside it — the "someone died here" scatter shared
    /// by surface generation and the spawn garden.</summary>
    /// <param name="isFree">Optional filter for the loot cells, e.g. to keep drops off
    /// occupied cells. Null allows any cell.</param>
    public static void SpawnSkeletonCorpse(World world, Vector3Int cell, System.Random rng,
        System.Func<Vector3Int, bool> isFree = null)
    {
        PlaceEntity(world, cell, ID.Skeleton);

        int count = rng.Next(1, 3);
        for (int i = 0; i < count; i++)
        {
            int x = cell.x + rng.Next(-1, 2);
            int z = cell.z + rng.Next(-1, 2);
            if (x < 1 || z < 1 || x >= world.Bounds.x - 1 || z >= world.Bounds.z - 1) continue;

            Vector3Int lootCell = new Vector3Int(x, cell.y, z);
            if (lootCell == cell) continue;
            if (isFree != null && !isFree(lootCell)) continue;

            // Loose items go to DynamicEntity, so they spawn as real ground drops.
            GenBlocks.PlaceEntity(world, lootCell, PickSkeletonLoot(rng));
        }
    }

    /// <summary>Low-tier starter loot found beside a skeleton.</summary>
    private static ID PickSkeletonLoot(System.Random rng)
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
