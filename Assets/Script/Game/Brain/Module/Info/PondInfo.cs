using System;
using UnityEngine;

[Serializable]
public class PondInfo : SpriteStructureInfo
{
    /// <summary>The liquid this pond holds; decides what filling a bucket yields.</summary>
    public LiquidType Liquid = LiquidType.Water;

    public override bool OnHitInternal(Projectile projectile)
    {
        if (LiquidRegistry.TryFillBucket(projectile.SourceInfo, Liquid, position))
            return true;
        return base.OnHitInternal(projectile);
    }
}
