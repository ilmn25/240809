using UnityEngine;

/// <summary>Places one or more raider camps: a central loot chest surrounded by
/// 2-3 dirty tents and a lamp (powered by a nearby generator so it glows).
/// Runs once per world, after chunk generation (like the graveyard).</summary>
public class GenTaskRaiderCamp : Gen
{
    private const int CampCount = 2;        // how many separate camps to place
    private const int MinTents = 2;
    private const int MaxTents = 3;
    private const int ClusterRadius = 4;    // how far structures scatter from the chest

    /// <summary>Places the raider camps, if terrain permits.</summary>
    public static void Run(World world)
    {
        System.Random rng = new System.Random((int)GetDeterministicOffset("RaiderCamp"));

        for (int i = 0; i < CampCount; i++)
        {
            Vector3Int center = PickCampCenter(world, rng);
            if (center.x < 0) continue;
            PlaceCamp(world, center, rng);
        }
    }

    /// <summary>Places a camp cluster around the given surface center.</summary>
    private static void PlaceCamp(World world, Vector3Int center, System.Random rng)
    {
        // Central loot chest (with the standard chest loot table).
        ContainerInfo chest = (ContainerInfo)Entity.CreateInfo(ID.Chest, center);
        Loot.Gettable(ID.Chest).AddToContainer(chest.Storage);
        PlaceInfo(world, center, chest);

        int tentCount = rng.Next(MinTents, MaxTents + 1);
        for (int i = 0; i < tentCount; i++)
        {
            Vector3Int spot = ScatterAround(world, center, rng);
            if (spot.x < 0) continue;
            PlaceEntity(world, spot, ID.DirtyTent);
        }

        // A lamp powered by a generator so it glows at the camp.
        Vector3Int lampSpot = ScatterAround(world, center, rng);
        Vector3Int genSpot = ScatterAround(world, center, rng);
        if (lampSpot.x >= 0) PlaceEntity(world, lampSpot, ID.Lamp);
        if (genSpot.x >= 0) PlaceEntity(world, genSpot, ID.Generator);
    }

    /// <summary>Returns a surface position within the cluster radius of the center,
    /// or (-1,0,0) if no valid surface is found nearby.</summary>
    private static Vector3Int ScatterAround(World world, Vector3Int center, System.Random rng)
    {
        for (int attempt = 0; attempt < 6; attempt++)
        {
            int x = center.x + rng.Next(-ClusterRadius, ClusterRadius + 1);
            int z = center.z + rng.Next(-ClusterRadius, ClusterRadius + 1);
            int surfaceY = FindSurfaceY(world, x, z);
            if (surfaceY < 0) continue;
            return new Vector3Int(x, surfaceY, z);
        }
        return new Vector3Int(-1, 0, 0);
    }

    /// <summary>Finds a random land column to center a camp on. Returns
    /// (-1,0,0) if none found.</summary>
    private static Vector3Int PickCampCenter(World world, System.Random rng)
    {
        for (int attempt = 0; attempt < 40; attempt++)
        {
            int x = rng.Next(2, world.Bounds.x - 2);
            int z = rng.Next(2, world.Bounds.z - 2);
            if (GenHelpBiome.GetBiomeType(x, z) != BiomeType.Grass) continue;
            int surfaceY = FindSurfaceY(world, x, z);
            if (surfaceY < 0) continue;
            return new Vector3Int(x, surfaceY, z);
        }
        return new Vector3Int(-1, 0, 0);
    }

    /// <summary>Adds a static entity to the chunk containing <paramref name="cell"/>.</summary>
    private static void PlaceEntity(World world, Vector3Int cell, ID id)
    {
        if (id == ID.Null) return;
        Chunk chunk = world[World.GetChunkCoordinate(cell)];
        if (chunk == null || chunk == Chunk.Zero) return;
        chunk.StaticEntity.Add(Entity.CreateInfo(id, cell));
    }

    /// <summary>Adds an already-created info (e.g. a filled chest) to its chunk.</summary>
    private static void PlaceInfo(World world, Vector3Int cell, Info info)
    {
        if (info == null) return;
        Chunk chunk = world[World.GetChunkCoordinate(cell)];
        if (chunk == null || chunk == Chunk.Zero) return;
        chunk.StaticEntity.Add(info);
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