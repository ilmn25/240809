using UnityEngine;

public abstract class GenTaskScatter : IGenTask
{
    public abstract void RunWorld(World world);

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

    protected static Vector3Int PickGrassCenter(World world, System.Random rng, int maxAttempts, System.Func<Vector3Int, bool> accept)
    {
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Vector3Int column = PickGrassCenter(world, rng);
            if (column.x < 0) return column;
            if (accept(column)) return column;
        }
        return new Vector3Int(-1, 0, 0);
    }

    protected static Vector3Int PickFootprintOrigin(World world, System.Random rng, int size, int maxSpread = 2, int maxAttempts = 30)
    {
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Vector3Int column = PickGrassCenter(world, rng);
            if (column.x < 0) return column;
            int surfaceY = FindFootprintSurface(world, column.x, column.z, size, maxSpread);
            if (surfaceY < 0) continue;
            return new Vector3Int(column.x, surfaceY, column.z);
        }
        return new Vector3Int(-1, 0, 0);
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

    protected static void PlaceEntity(World world, Vector3Int cell, ID id)
    {
        if (id == ID.Null) return;
        PlaceInfo(world, cell, Entity.CreateInfo(id, cell));
    }

    protected static void PlaceInfo(World world, Vector3Int cell, Info info)
    {
        if (info == null) return;
        Chunk chunk = world[World.GetChunkCoordinate(cell)];
        if (chunk == null || chunk == Chunk.Zero) return;
        chunk.StaticEntity.Add(info);
    }

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
