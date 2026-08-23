using UnityEngine;

/// <summary>Places an owl statue beside the world's spawn point. Because NPCs
/// aren't saved, the statue (a saved static structure) respawns the Guide.
/// Runs once per world, after chunk generation, for the Abyss dimension.</summary>
public class GenTaskSpawnStatue : GenTaskScatter
{
    /// <summary>Places the spawn statue, if the surface beside spawn exists.</summary>
    public static void Run(World world)
    {
        Vector3Int spawnPos = world.SpawnPoint;

        // Snap the spot beside spawn down onto the ground so the statue never floats.
        int x = spawnPos.x + 2;
        int z = spawnPos.z;
        int surfaceY = FindSurfaceY(world, x, z);
        if (surfaceY < 0) return;

        PlaceEntity(world, new Vector3Int(x, surfaceY, z), ID.OwlStatue);
    }
}
