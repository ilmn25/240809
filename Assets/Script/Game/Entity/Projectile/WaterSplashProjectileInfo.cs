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

        int hitCount = Physics.OverlapSphereNonAlloc(center, Radius, HitBuffer, Main.MaskEntity);
        bool wateredAny = false;
        for (int i = 0; i < hitCount; i++)
        {
            Machine machine = HitBuffer[i].GetComponent<Machine>();
            if (machine == null || (projectile.SourceInfo != null && machine == projectile.SourceInfo.Machine)) continue;
            if (machine is PlanterMachine planter && planter.Water())
                wateredAny = true;
        }

        // Ring of hit-dust puffs reads as water splashing onto the ground.
        for (int i = 0; i < 5; i++)
        {
            Vector3 offset = new Vector3(Random.value - 0.5f, 0.4f, Random.value - 0.5f) * Radius;
            Particle.Create(center + offset, Particles.HitDust, false);
        }

        if (wateredAny)
        {
            Audio.PlaySFX(SfxID.Sword);
            if (projectile.SourceInfo is PlayerInfo player &&
                player.Storage?.List != null && player.Storage.List.Count > 0)
            {
                ItemSlot selected = player.Storage.GetSelected();
                if (selected != null && selected.Stack > 0 && selected.ID == ID.BucketOfWater)
                {
                    player.Storage.List[player.Storage.Key] = new ItemSlot(ID.Bucket, 1);
                    player.Storage.NotifyChanged();
                }
            }
        }

        projectile.Delete();
    }
}
