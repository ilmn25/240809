/// <summary>A barrel that can hold a liquid. Swinging an empty bucket at a filled
/// barrel collects the liquid into the bucket and empties the barrel.</summary>
public class BarrelInfo : SpriteStructureInfo
{
    /// <summary>The liquid currently stored in the barrel. None = empty barrel.</summary>
    public LiquidType Liquid = LiquidType.None;

    public override bool OnHitInternal(Projectile projectile)
    {
        if (Machine is not BarrelMachine barrel) return base.OnHitInternal(projectile);
        // Pour a held filled bucket in, or collect the liquid with an empty one.
        if (barrel.TryPour(projectile.SourceInfo)) return true;
        if (barrel.TryCollect(projectile.SourceInfo)) return true;
        return base.OnHitInternal(projectile);
    }
}
