using UnityEngine;

/// <summary>Info for harvestable plants/decor. Can be harvested by attacking with
/// any tool — no OperationType/Breaking requirement. Behavior is data-driven via
/// HarvestableRegistry: each harvestable ID defines its drops and whether it is
/// destroyed on harvest.</summary>
[System.Serializable]
public class HarvestableInfo : SpriteStructureInfo
{
    public override bool OnHitInternal(Projectile projectile)
    {
        if (projectile.SourceInfo.Equipment == null ||
            projectile.SourceInfo.targetHitboxType == HitboxType.Player)
            return false;

        HarvestableDefinition definition = HarvestableRegistry.Get(id);
        if (definition == null)
            return false;

        // Harvest directly here without setting the attacker's target, so the
        // swing animation isn't reset by re-targeting this destroyed harvestable.
        Particle.Create(position, Particles.HitDust, false);

        // Roll the drop table.
        definition.Drops?.Spawn(position);

        Audio.PlaySFX(SfxID.Item);

        // Some harvestables (berry bush) stay so they can be harvested again.
        if (definition.DestroyOnHarvest)
            Destroy();

        return true;
    }
}
