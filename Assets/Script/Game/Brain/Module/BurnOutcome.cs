using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Describes what happens when a flammable object burns out. Each outcome is
/// polymorphic so new burn behaviors can be added without touching the core
/// fire system. Outcomes are looked up by entity ID in BurnOutcomeRegistry.
/// </summary>
public abstract class BurnOutcome
{
    /// <summary>Apply this outcome at the burned-out position.</summary>
    public abstract void Apply(Vector3 position);
}

/// <summary>Spawns a static structure at the burn site (tree → burned tree, structure → rubble).</summary>
public class SpawnStructureOutcome : BurnOutcome
{
    public ID StructureID;
    public override void Apply(Vector3 position)
    {
        Entity.Spawn(StructureID, Vector3Int.FloorToInt(position));
    }
}

/// <summary>Drops an item at the burn site (decor → ash).</summary>
public class DropItemOutcome : BurnOutcome
{
    public ID ItemID;
    public int Amount = 1;
    public override void Apply(Vector3 position)
    {
        Entity.SpawnItem(ItemID, position, Amount);
    }
}

/// <summary>Runs several outcomes at once (e.g. rubble + ash).</summary>
public class CompositeOutcome : BurnOutcome
{
    public List<BurnOutcome> Outcomes = new List<BurnOutcome>();
    public override void Apply(Vector3 position)
    {
        foreach (BurnOutcome outcome in Outcomes)
            outcome.Apply(position);
    }
}
