using System.Collections.Generic;

/// <summary>
/// Data-driven definition for a harvestable plant/decor. Each harvestable ID
/// maps to one of these in HarvestableRegistry. Adding a new harvestable is
/// just registering one entry — no per-entity logic needed.
/// </summary>
public class HarvestableDefinition
{
    /// <summary>Drop table rolled on every harvest. Null/empty = drops nothing.</summary>
    public Loot Drops;
    /// <summary>If true, the harvestable is destroyed after being harvested.
    /// If false, it stays so it can be harvested repeatedly.</summary>
    public bool DestroyOnHarvest = true;
    /// <summary>If true, this harvestable can catch and spread fire.</summary>
    public bool Flammable = false;

    public HarvestableDefinition(bool destroyOnHarvest = true, bool flammable = false)
    {
        DestroyOnHarvest = destroyOnHarvest;
        Flammable = flammable;
    }

    /// <summary>Convenience: create a definition with a drop table.</summary>
    public HarvestableDefinition(Loot drops, bool destroyOnHarvest = true, bool flammable = false)
    {
        Drops = drops;
        DestroyOnHarvest = destroyOnHarvest;
        Flammable = flammable;
    }
}

/// <summary>
/// Central, data-driven registry of all harvestable entities. Each harvestable
/// ID maps to a HarvestableDefinition describing its drops and behavior.
/// Adding a new harvestable is just one entry here.
/// </summary>
public static class HarvestableRegistry
{
    private static readonly Dictionary<ID, HarvestableDefinition> Map = new Dictionary<ID, HarvestableDefinition>();

    /// <summary>Register all harvestable definitions. Called once at startup.</summary>
    public static void Initialize()
    {
        // Berry bush: hit to knock off sticks and rarely berries, but the bush
        // stays so it can be harvested again.
        Loot bush = new Loot(ID.Bush);
        bush.Add(1f, 1, ID.Sticks);
        bush.Add(0.2f, 1, ID.Berries);
        Register(ID.Bush, new HarvestableDefinition(bush, destroyOnHarvest: false, flammable: true));

        // Grass: flammable, drops nothing when harvested (just decor).
        Register(ID.Grass, new HarvestableDefinition(flammable: true));

        // Flowers: drop themselves and are consumed.
        Loot deathcap = new Loot(ID.Deathcap);
        deathcap.Add(1f, 1, ID.Deathcap);
        Register(ID.Deathcap, new HarvestableDefinition(deathcap));

        Loot orchids = new Loot(ID.Orchids);
        orchids.Add(1f, 1, ID.Orchids);
        Register(ID.Orchids, new HarvestableDefinition(orchids));

        // Wooden table: flammable decor, drops nothing.
        Register(ID.Table, new HarvestableDefinition(flammable: true));
    }

    /// <summary>Register (or override) a harvestable definition.</summary>
    public static void Register(ID id, HarvestableDefinition definition)
    {
        Map[id] = definition;
    }

    /// <summary>Get the definition for a harvestable, or null if it isn't one.</summary>
    public static HarvestableDefinition Get(ID id)
    {
        return Map.GetValueOrDefault(id);
    }
}
