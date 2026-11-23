[System.Serializable]
public class HarvestableInfo : SpriteStructureInfo
{
    /// <summary>Seconds until this harvestable regrows after being picked
    /// (bush cooldown). Persisted so the picked state survives save/load.</summary>
    public float RegrowTimer;

    public HarvestableInfo()
    {
        SpawnsRubble = false;
    }

    public override bool OnHitInternal(Projectile projectile)
    {
        if (projectile.SourceInfo is not PlayerInfo player || player.Equipment == null)
            return false;

        HarvestableDefinition definition = HarvestableRegistry.Get(id);
        if (definition == null)
            return false;

        if (RegrowTimer > 0f)
            return true;

        if (!definition.CanHarvestWith(player.Equipment.Info.ProjectileInfo))
            return false;

        Particle.Create(position, Particles.HitDust, false);
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
