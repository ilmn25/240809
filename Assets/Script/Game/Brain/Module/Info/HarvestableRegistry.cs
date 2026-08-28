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
    /// <summary>Seconds before a non-destroyed harvestable regrows and can be
    /// harvested again (0 = always harvestable). Prevents infinite farming.</summary>
    public float RegrowTime = 0f;

    public HarvestableDefinition(bool destroyOnHarvest = true, float regrowTime = 0f)
    {
        DestroyOnHarvest = destroyOnHarvest;
        RegrowTime = regrowTime;
    }

    /// <summary>Convenience: create a definition with a drop table.</summary>
    public HarvestableDefinition(Loot drops, bool destroyOnHarvest = true, float regrowTime = 0f)
    {
        Drops = drops;
        DestroyOnHarvest = destroyOnHarvest;
        RegrowTime = regrowTime;
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
        bush.Add(1f, 1, ID.Berries);
        bush.Add(0.5f, 1, ID.Berries);
        Register(ID.Bush, new HarvestableDefinition(bush, destroyOnHarvest: false, regrowTime: 20f));

        // Grass: flammable decor that rarely yields sticks/flint (1/25) or a bullet casing (1/100).
        Loot grass = new Loot(ID.Grass);
        grass.Add(0.04f, 1, ID.Sticks, ID.Flint);
        grass.Add(0.01f, 1, ID.Casing);
        Register(ID.Grass, new HarvestableDefinition(grass));

        // Flowers: drop themselves and are consumed.
        Loot deathcap = new Loot(ID.Deathcap);
        deathcap.Add(1f, 1, ID.Deathcap);
        Register(ID.Deathcap, new HarvestableDefinition(deathcap));

        Loot orchids = new Loot(ID.Orchids);
        orchids.Add(1f, 1, ID.Orchids);
        Register(ID.Orchids, new HarvestableDefinition(orchids));

        Loot tulip = new Loot(ID.Tulip);
        tulip.Add(1f, 1, ID.Tulip);
        Register(ID.Tulip, new HarvestableDefinition(tulip));

        Loot daisies = new Loot(ID.Daisies);
        daisies.Add(1f, 1, ID.Daisies);
        Register(ID.Daisies, new HarvestableDefinition(daisies));

        // Wooden table: flammable decor that burns down to its wooden materials.
        Loot table = new Loot(ID.Table);
        table.Add(1f, 2, ID.Plank);
        table.Add(0.5f, 2, ID.Sticks);
        Register(ID.Table, new HarvestableDefinition(table));

        // Spider web: cut down for sticky silk, consumed on harvest.
        Loot web = new Loot(ID.SpiderWeb);
        web.Add(1f, 1, ID.SpiderWeb);
        Register(ID.SpiderWeb, new HarvestableDefinition(web));

        // Skeleton: break apart for bone loot, including the femur weapon.
        Loot skeleton = new Loot(ID.Skeleton);
        skeleton.Add(0.4f, 1, ID.Flint);
        skeleton.Add(0.35f, 1, ID.Femur);
        Register(ID.Skeleton, new HarvestableDefinition(skeleton));

        // Old pot: smash to either pop a viper or spill loot (the either/or is
        // handled by OldPotInfo.OnHarvest).
        Loot pot = new Loot(ID.OldPot);
        pot.Add(0.7f, 1, ID.Gold, ID.Copper);
        pot.Add(0.4f, 1, ID.Steel, ID.Slag);
        pot.Add(0.3f, 1, ID.Foul, ID.Bandages);
        Register(ID.OldPot, new HarvestableDefinition(pot));

        // Fallen tree: chop the dead wood for timber, consumed on harvest.
        // Single table for this id — registered here so the burn path
        // (CombustionRegistry.BurnStructure(ID.FallenTree)) finds it by id too.
        // Harvest and burn therefore drop the same thing.
        Loot fallenTree = new Loot(ID.FallenTree);
        fallenTree.Add(1f, 2, ID.Log);
        fallenTree.Add(0.5f, 2, ID.Log);
        fallenTree.Add(1f, 1, ID.Sticks);
        fallenTree.Add(0.5f, 1, ID.Acorn);
        Register(ID.FallenTree, new HarvestableDefinition(fallenTree));

        // Mud pile: dig it out for mud and the occasional flint/gravel, consumed on harvest.
        Loot mudPile = new Loot(ID.MudPile);
        mudPile.Add(1f, 3, ID.Mud);
        mudPile.Add(0.5f, 2, ID.Mud);
        mudPile.Add(0.3f, 1, ID.Flint);
        mudPile.Add(0.3f, 1, ID.Gravel);
        Register(ID.MudPile, new HarvestableDefinition(mudPile));
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
