using UnityEngine;

/// <summary>A thrown explosive. It arcs with gravity, then detonates when the
/// fuse runs out or on hitting a wall, the floor, or a creature. The blast is
/// attributed to the thrower but spares them (no point-blank self-harm).
/// Per-flight state lives on the Projectile component (Velocity, LifeSpan) so
/// the shared weapon info instance is safe for several grenades in flight.</summary>
public class GrenadeProjectileInfo : RangedProjectileInfo
{
    /// <summary>Downward acceleration for the throw arc.</summary>
    public float Gravity = -24f;
    /// <summary>Frames of ignored collision at spawn so the grenade can leave the
    /// thrower's hand before it starts checking the floor.</summary>
    public int SpawnGraceFrames = 3;
    /// <summary>Blast spawned where the grenade detonates.</summary>
    public ExplosionProjectileInfo Blast;

    public override void OnSpawn(Projectile projectile)
    {
        base.OnSpawn(projectile);
        // Toss it up a touch so it arcs over low obstacles, then gravity pulls it down.
        projectile.Velocity = projectile.Direction * Speed + Vector3.up * (Speed * 0.7f);
    }

    public override void AI(Projectile projectile)
    {
        // Fuse ran out -> detonate instead of just fading away.
        if (projectile.LifeSpan > LifeSpan)
        {
            Detonate(projectile);
            return;
        }

        projectile.Velocity.y += Gravity * Time.deltaTime;
        projectile.transform.position += projectile.Velocity * Time.deltaTime;

        // Leave the thrower's hand before checking for collisions.
        if (projectile.LifeSpan < SpawnGraceFrames) return;

        int hitCount = Physics.OverlapSphereNonAlloc(projectile.transform.position, Radius, HitBuffer);
        for (int i = 0; i < hitCount; i++)
        {
            // Wall or floor -> detonate.
            if (Helper.IsInLayerMask(HitBuffer[i].gameObject, Main.MaskMap))
            {
                Detonate(projectile);
                return;
            }

            // Any creature (mob, player, structure) -> detonate. Skip the thrower.
            Machine machine = HitBuffer[i].GetComponent<Machine>();
            if (machine == null) continue;
            if (projectile.SourceInfo != null && machine == projectile.SourceInfo.Machine) continue;
            Detonate(projectile);
            return;
        }
    }

    private void Detonate(Projectile projectile)
    {
        if (Blast != null)
        {
            // +forward so Direction isn't zero (avoids a LookRotation warning);
            // direction is irrelevant for an instant area blast.
            Projectile.Spawn(projectile.transform.position, projectile.transform.position + Vector3.forward, Blast, HitboxType.All, projectile.SourceInfo);
        }
        projectile.Delete();
    }
}
