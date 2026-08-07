using UnityEngine;

/// <summary>Info for harvestable plants/decor. Can be harvested by attacking with
/// any tool — no OperationType/Breaking requirement.</summary>
[System.Serializable]
public class HarvestableInfo : SpriteStructureInfo
{
    public override bool OnHitInternal(Projectile projectile)
    {
        if (projectile.SourceInfo.Equipment == null ||
            projectile.SourceInfo.targetHitboxType == HitboxType.Player)
            return false;

        // Harvest directly here without setting the attacker's target, so the
        // swing animation isn't reset by re-targeting this destroyed harvestable.
        Particle.Create(position, Particles.HitDust, false);
        if (id == ID.Deathcap || id == ID.Orchids)
            Entity.SpawnItem(id, position);

        Audio.PlaySFX(SfxID.Item);
        Destroy();
        return true;
    }
}
