using UnityEngine;

/// <summary>A flaming arrow. In addition to its arrow damage, it ignites whatever
/// it hits (structures, mobs, players) so it catches fire.</summary>
public class FlameProjectile : RangedProjectileInfo
{
    protected override void OnHitTarget(Projectile projectile, Machine target)
    {
        FlammableModule flammable = target.GetModule<FlammableModule>();
        if (flammable != null && flammable.Ignite())
            Particle.Create(target.transform.position + new Vector3(0, 0.5f, 0), Particles.Fire, false);
    }
}