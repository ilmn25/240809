using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Describes what happens when a flammable object burns out. Each outcome is
/// polymorphic so new burn behaviors can be added without touching the core
/// fire system. Outcomes are looked up by entity ID in CombustionRegistry.
/// </summary>
public abstract class BurnOutcome
{
    /// <summary>Apply this outcome at the burned-out position.</summary>
    public abstract void Apply(Vector3 position);
}

/// <summary>Spawns a static structure at the burn site (tree → burned tree).</summary>
public class SpawnStructureOutcome : BurnOutcome
{
    public ID StructureID;
    public override void Apply(Vector3 position)
    {
        Entity.Spawn(StructureID, Vector3Int.FloorToInt(position));
    }
}

/// <summary>Drops an item at the burn site (plant → ash).</summary>
public class DropItemOutcome : BurnOutcome
{
    public ID ItemID;
    public int Amount = 1;
    public override void Apply(Vector3 position)
    {
        Entity.SpawnItem(ItemID, position, Amount);
    }
}

/// <summary>Drops the burned entity's loot table, converting any burnable loot
/// (wood, plants) to its burn result (charcoal).</summary>
public class ConvertedLootOutcome : BurnOutcome
{
    public ID LootID;
    public override void Apply(Vector3 position)
    {
        if (LootID == ID.Null || !Loot.TryGet(LootID, out Loot table)) return;
        table.SpawnBurned(position);
    }
}

/// <summary>Runs several outcomes at once (e.g. charred rubble + loot drops).</summary>
public class CompositeOutcome : BurnOutcome
{
    public List<BurnOutcome> Outcomes = new List<BurnOutcome>();
    public override void Apply(Vector3 position)
    {
        foreach (BurnOutcome outcome in Outcomes)
            outcome.Apply(position);
    }
}
