using UnityEngine;

/// <summary>Stands a threshold portal in the middle of the world (its spawn point) —
/// the way into the backrooms. Runs once per world, after chunk generation, for the
/// Abyss dimension (its twin waits in the middle of <see cref="GenBackrooms"/>).</summary>
public class GenTaskSpawnPortal : GenTaskScatter
{
    /// <summary>Places the portal on the ground at the world's spawn point.</summary>
    public override void RunWorld(World world)
    {
        Vector3Int spawnPos = world.SpawnPoint;
        int surfaceY = FindSurfaceY(world, spawnPos.x, spawnPos.z);
        if (surfaceY < 0) return;

        PlaceEntity(world, new Vector3Int(spawnPos.x, surfaceY, spawnPos.z), ID.Threshold);
    }
}
