using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// A melee water splash. Instead of damaging, it waters every planter in the
/// arc, empties the held water bucket, and kicks up a ring of splash particles.
/// </summary>
public class WaterSplashProjectileInfo : SwingProjectileInfo
{
    public WaterSplashProjectileInfo() { Class = ProjectileClass.Melee; }

    public override void AI(Projectile projectile)
    {
        // Player swings aim the splash ahead of the swing arc; a stationary
        // source (sprinkler) splashes right where it is.
        Vector3 direction = (projectile.Destination - projectile.transform.position).normalized;
        Vector3 center = projectile.SourceInfo != null
            ? projectile.transform.position + direction * (Radius * 0.5f)
            : projectile.transform.position;

        bool wateredAny = false;
        ForEachMachine(projectile, center, Radius, (machine, _) =>
        {
            if (machine is PlanterMachine planter && planter.Water())
                wateredAny = true;
        });

        for (int i = 0; i < 5; i++)
        {
            Vector3 offset = new Vector3(Random.value - 0.5f, 0.4f, Random.value - 0.5f) * Radius;
            Particle.Create(center + offset, Particles.HitDust, false);
        }

        if (wateredAny)
        {
            Audio.PlaySFX(SfxID.Sword);
            if (projectile.SourceInfo is PlayerInfo player &&
                player.HeldSlot is { Stack: > 0 } held && held.ID == ID.BucketOfWater)
            {
                Inventory.ReplaceHeld(player, ID.Bucket);
            }
        }

        projectile.Delete();
    }
}
