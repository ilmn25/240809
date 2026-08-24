using System.Collections.Generic;

/// <summary>A fixed item dropped at the burn site.</summary>
public class BurnDrop
{
    public ID ItemID;
    public int Amount = 1;
    public BurnDrop(ID itemID, int amount = 1) { ItemID = itemID; Amount = amount; }
}

/// <summary>
/// Data-driven combustion profile for a placed entity: whether it can catch and
/// spread fire, and what it yields when it burns out. Single source of truth for
/// flammability and burn output — no per-machine flags, no polymorphic outcome
/// classes.
/// </summary>
public class CombustionProfile
{
    public bool Flammable;
    /// <summary>Structure left at the burn site (BurnedTree, CharredRubble). ID.Null = none.</summary>
    public ID Spawns = ID.Null;
    /// <summary>Loot table rolled at the burn site, burnable items converted. ID.Null = none.</summary>
    public ID DropsLoot = ID.Null;
    /// <summary>Fixed items dropped at the burn site (e.g. ash).</summary>
    public List<BurnDrop> Drops = new List<BurnDrop>();

    public CombustionProfile(bool flammable = false) { Flammable = flammable; }
}

/// <summary>
/// Central, data-driven registry of combustion profiles. Each flammable entity
/// is registered here once with its flammability and burn output. Adding a new
/// flammable thing is a single entry.
/// </summary>
public static class CombustionRegistry
{
    private static readonly Dictionary<ID, CombustionProfile> Map = new Dictionary<ID, CombustionProfile>();

    public static void Initialize()
    {
        // Trees: catch fire and collapse to a charred tree you can chop for charcoal.
        Register(ID.PineTree,  new CombustionProfile(true) { Spawns = ID.BurnedTree });
        Register(ID.BirchTree, new CombustionProfile(true) { Spawns = ID.BurnedTree });
        Register(ID.OakTree,   new CombustionProfile(true) { Spawns = ID.BurnedTree });

        // Wooden structures: collapse into charred rubble and drop their loot,
        // burnable wood converting to charcoal.
        BurnStructure(ID.Workbench);
        BurnStructure(ID.FieldStation);
        BurnStructure(ID.Sawmill);
        BurnStructure(ID.ImprovisedPlanter);
        BurnStructure(ID.Bed);
        BurnStructure(ID.Sign);
        BurnStructure(ID.Table);

        // Plants and decor all smolder down to ash.
        Ash(ID.Bush, ID.Grass, ID.Deathcap, ID.Orchids);

        // Oil barrel: flammable, but it doesn't burn out normally — it explodes
        // instead (OilBarrelMachine handles the explosion).
        Register(ID.OilBarrel, new CombustionProfile(true));
    }

    /// <summary>Wooden structure: leaves charred rubble and drops its loot with
    /// burnable wood converting to charcoal.</summary>
    private static void BurnStructure(ID id) =>
        Register(id, new CombustionProfile(true) { Spawns = ID.CharredRubble, DropsLoot = id });

    /// <summary>Register several flammable entities that all burn down to ash.</summary>
    private static void Ash(params ID[] ids)
    {
        foreach (ID id in ids)
            Register(id, new CombustionProfile(true) { Drops = { new BurnDrop(ID.Ash) } });
    }

    private static void Register(ID id, CombustionProfile profile) => Map[id] = profile;

    public static CombustionProfile Get(ID id) => Map.GetValueOrDefault(id);
}
