using System;
using UnityEngine;

[Serializable]
public class PondInfo : SpriteStructureInfo
{
    /// <summary>The liquid this pond holds; decides what filling a bucket yields.</summary>
    public LiquidType Liquid = LiquidType.Water;

    public override bool OnHitInternal(Projectile projectile)
    {
        if (TryFillBucket(projectile.SourceInfo))
            return true;
        return base.OnHitInternal(projectile);
    }

    private bool TryFillBucket(MobInfo source)
    {
        if (source is not PlayerInfo player ||
            player.Storage?.List == null || player.Storage.List.Count == 0)
            return false;
        if (!LiquidRegistry.TryGetFilledBucket(Liquid, out ID filled))
            return false;

        int key = Mathf.Clamp(player.Storage.Key, 0, player.Storage.List.Count - 1);
        ItemSlot selected = player.Storage.List[key];
        if (selected.Stack <= 0 || selected.ID != LiquidRegistry.EmptyBucket)
            return false;

        player.Storage.List[key] = new ItemSlot(filled, 1);
        player.Storage.NotifyChanged();

        Particle.Create(position + Vector3.up * 0.5f, Particles.HitDust, false);
        return true;
    }
}
