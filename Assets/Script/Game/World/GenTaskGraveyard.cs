using UnityEngine;

/// <summary>Places a single graveyard cluster in the grass biome: a few
/// headstones scattered around a deterministic center point. Runs once per
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

            Vector3Int spot = new Vector3Int(x, surfaceY, z);
            Chunk chunk = world[World.GetChunkCoordinate(spot)];
            if (chunk == null || chunk == Chunk.Zero) continue;

            chunk.StaticEntity.Add(Entity.CreateInfo(ID.Headstone, spot));

            // A buried skeleton lies just under each headstone, with scattered loot.
            Vector3Int grave = new Vector3Int(x, surfaceY - 1, z);
            Chunk graveChunk = world[World.GetChunkCoordinate(grave)];
            if (graveChunk != null && graveChunk != Chunk.Zero)
            {
                graveChunk.StaticEntity.Add(Entity.CreateInfo(ID.Skeleton, grave));
                SpawnGraveLoot(world, grave, rng);
            }
        }
    }

    // Scatters low-tier loot around the grave, one block under the surface.
    private static void SpawnGraveLoot(World world, Vector3Int grave, System.Random rng)
    {
        int count = rng.Next(1, 3);
        for (int i = 0; i < count; i++)
        {
            Vector3Int lootPos = new Vector3Int(
                grave.x + rng.Next(-1, 2),
                grave.y,
                grave.z + rng.Next(-1, 2));
            Chunk chunk = world[World.GetChunkCoordinate(lootPos)];
            if (chunk == null || chunk == Chunk.Zero) continue;

            ID item = PickGraveLoot(rng);
            if (item != ID.Null)
                chunk.DynamicEntity.Add(Entity.CreateInfo(item, lootPos));
        }
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
