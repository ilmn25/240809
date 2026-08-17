using UnityEngine;

/// <summary>Info for furniture: a static structure that can't be damaged or broken,
/// placed directly (no build phase). Hitting it with a hammer (Building) drops it
/// back to the ground as an item.</summary>
public class FurnitureInfo : StructureInfo
{
    public override bool OnHitInternal(Projectile projectile)
    {
        // Furniture ignores enemies and every tool except the hammer (Building).
        if (projectile.SourceInfo.HitboxType == HitboxType.Enemy) return false;
        if (projectile.SourceInfo.Equipment == null ||
            projectile.SourceInfo.Equipment.Info.ProjectileInfo?.OperationType != OperationType.Building)
            return false;
        // User-controlled players acquire the target from hitting it, so the
        // swing's AbstractHit (pickup) fires on this entity.
        if (projectile.SourceInfo is not PlayerInfo || projectile.SourceInfo.controllerId != -1)
            projectile.SourceInfo.AcquireTarget(this);
        return true;
    }

    public override void AbstractHit(MobInfo info)
    {
        // Enemies and non-hammer tools do nothing to furniture.
        if (info.HitboxType == HitboxType.Enemy) return;
        if (info.Equipment == null ||
            info.Equipment.Info.ProjectileInfo?.OperationType != OperationType.Building)
            return;

        Audio.PlaySFX(SfxDestroy);
        Entity.SpawnItem(Loot, position);
        RemoveFromChunk();
        Destroy();
    }
}
