using UnityEngine;

/// <summary>Info for harvestable plants/decor. Can be harvested by attacking with
/// any tool — no OperationType/Breaking requirement. Behavior is data-driven via
/// HarvestableRegistry: each harvestable ID defines its drops and whether it is
/// destroyed on harvest.</summary>
[System.Serializable]
public class HarvestableInfo : SpriteStructureInfo
{
    /// <summary>Seconds until this harvestable regrows after being picked
    /// (bush cooldown). Persisted so the picked state survives save/load.</summary>
    public float RegrowTimer;

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

        // Picked and regrowing — no drops yet.
        if (RegrowTimer > 0f)
            return true;

        // Defer the actual harvest (drops/destroy/regrow) to a virtual hook so
        // subclasses can add bespoke behavior (e.g. the old pot popping a viper).
        OnHarvest(definition);

        return true;
    }

    /// <summary>What happens when this harvestable is harvested. Default rolls
    /// the drop table, then destroys it or starts the regrow timer. Subclasses
    /// can override, calling base to keep the default.</summary>
    protected virtual void OnHarvest(HarvestableDefinition definition)
    {
        definition.Drops?.Spawn(position);

        Audio.PlaySFX(SfxID.Item);

        // Some harvestables (berry bush) stay but need time to regrow before
        // they can be harvested again; others are consumed. Either way they
        // aren't an infinite source of drops.
        if (definition.DestroyOnHarvest)
            Destroy();
        else if (definition.RegrowTime > 0f)
            RegrowTimer = definition.RegrowTime;
    }
}
