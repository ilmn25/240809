/// <summary>A flaming arrow. In addition to its arrow damage, it ignites whatever
/// it hits (structures, mobs, players) so it catches fire.</summary>
public class FlameProjectile : RangedProjectileInfo
{
    protected override void OnHitTarget(Projectile projectile, Machine target)
    {
        // Igniting applies the Burning status, which draws the flames itself.
        target.GetModule<FlammableModule>()?.Ignite();
    }
}