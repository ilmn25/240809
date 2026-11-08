/// <summary>Info for furniture: a static structure that can't be damaged or broken,
/// placed directly (no build phase). Hitting it with a mallet lifts it back intact.</summary>
[System.Serializable]
public class FurnitureInfo : StructureInfo
{
    // Furniture can always be lifted, even when its item has no craft recipe.
    protected override bool IsDismantlable => true;

    public override bool OnHitInternal(Projectile projectile)
    {
        // Enemies can't touch furniture; only a mallet lifts it.
        if (!IsDismantlingHit(projectile.SourceInfo)) return false;
        return base.OnHitInternal(projectile);
    }

    public override void AbstractHit(Projectile projectile)
    {
        if (!IsDismantlingHit(projectile.SourceInfo)) return;
        base.AbstractHit(projectile);
    }
}
