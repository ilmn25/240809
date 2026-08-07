using System.Collections.Generic;

/// <summary>
/// Central, data-driven registry mapping each flammable entity ID to the
/// BurnOutcome it produces when it burns out. Adding a new burn behavior is
/// just adding one entry here — no changes to the fire system or entities.
/// </summary>
public static class BurnOutcomeRegistry
{
    private static readonly Dictionary<ID, BurnOutcome> Map = new Dictionary<ID, BurnOutcome>
    {
        // Trees become charred burned trees, with a rare chance to drop charcoal.
        [ID.PineTree]  = new CompositeOutcome {
            Outcomes = {
                new SpawnStructureOutcome { StructureID = ID.BurnedTree },
                new ChanceDropItemOutcome { ItemID = ID.Charcoal, Amount = 1, Chance = 0.1f },
            }
        },
        [ID.BirchTree] = new CompositeOutcome {
            Outcomes = {
                new SpawnStructureOutcome { StructureID = ID.BurnedTree },
                new ChanceDropItemOutcome { ItemID = ID.Charcoal, Amount = 1, Chance = 0.1f },
            }
        },

        // Wooden structures collapse into rubble.
        [ID.Workbench]  = new SpawnStructureOutcome { StructureID = ID.Rubble },
        [ID.WoodenToolbench] = new SpawnStructureOutcome { StructureID = ID.Rubble },
        [ID.CarpenterWorkbench] = new SpawnStructureOutcome { StructureID = ID.Rubble },
        [ID.Loom]       = new SpawnStructureOutcome { StructureID = ID.Rubble },
        [ID.Sawmill]    = new SpawnStructureOutcome { StructureID = ID.Rubble },
        [ID.Campfire]   = new SpawnStructureOutcome { StructureID = ID.Rubble },
        [ID.Furnace]    = new SpawnStructureOutcome { StructureID = ID.Rubble },
        [ID.Table]      = new SpawnStructureOutcome { StructureID = ID.Rubble },

        // Decor and plants turn to ash.
        [ID.Bush]    = new DropItemOutcome { ItemID = ID.Ash },
        [ID.Grass]   = new DropItemOutcome { ItemID = ID.Ash },
        [ID.Deathcap] = new DropItemOutcome { ItemID = ID.Ash },
        [ID.Orchids] = new DropItemOutcome { ItemID = ID.Ash },
    };

    /// <summary>Returns the burn outcome for an entity, or null if it has none.</summary>
    public static BurnOutcome Get(ID id)
    {
        return Map.GetValueOrDefault(id);
    }
}
