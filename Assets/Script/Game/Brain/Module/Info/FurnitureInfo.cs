using UnityEngine;

/// <summary>Info for furniture: a static structure that can't be damaged or broken,
/// placed directly (no build phase). Hitting it with a hammer (Building) drops it
/// back to the ground as an item.</summary>
[System.Serializable]
public class FurnitureInfo : StructureInfo
{
    public override bool OnHitInternal(Projectile projectile)
    {
        // Furniture ignores enemies and every tool except the hammer (Building).
        if (projectile.SourceInfo.HitboxType == HitboxType.Enemy) return false;
        if (projectile.SourceInfo.Equipment == null ||
            projectile.SourceInfo.Equipment.Info.ProjectileInfo?.OperationType != OperationType.Building)
            return false;
        // Only a human-controlled player acquires the target here; an AI player's own
        // hit-acquisition policy decides (PlayerInfo.AcquireTarget).
        projectile.SourceInfo.AcquireTarget(this);
        return true;
    }

    public override void AbstractHit(Projectile projectile)
    {
        MobInfo info = projectile.SourceInfo;
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
