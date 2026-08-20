using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class ConverterInfo : ContainerInfo
{
    protected virtual IReadOnlyList<ID> GetOutputs(ID input) => null;
    protected virtual int OutputAmount(ID input) => 1;
    protected virtual int ConvertTime => 90;

    [NonSerialized] private int _counter;

    public virtual bool IsConverting()
    {
        if (Storage == null || Storage.List == null) return false;
        foreach (ItemSlot slot in Storage.List)
        {
            if (slot.isEmpty()) continue;
            if (GetOutputs(slot.ID) != null) return true;
        }
        return false;
    }

    protected bool CanProcess()
    {
        if (!Helper.IsHost()) return false;
        if (Storage == null || Storage.List == null) return false;
        return true;
    }

    /// <summary>Restarts the process timer (called when the container menu closes so cooking starts fresh).</summary>
    public void ResetTimer() => _counter = 0;

    /// <summary>Advances the process timer; returns true once the convert interval has elapsed.</summary>
    protected bool Tick()
    {
        if (_counter == ConvertTime)
        {
            _counter = 0;
            return true;
        }
        _counter++;
        return false;
    }

    protected void SpawnOutput(ID id, int amount = 1) =>
        Entity.SpawnItem(id, Machine.transform.position + OutputOffset(), amount, stackOnSpawn: false);

    public override void Update()
    {
        if (!CanProcess()) return;

        bool changed = false;
        foreach (ItemSlot slot in Storage.List)
        {
            if (slot.isEmpty()) continue;
            if (GetOutputs(slot.ID) != null) continue;
            Entity.SpawnItem(slot, Machine.transform.position + OutputOffset());
            slot.clear();
            changed = true;
        }
        if (changed)
            Storage.NotifyChanged();

        ItemSlot target = null;
        foreach (ItemSlot slot in Storage.List)
        {
            if (slot.isEmpty()) continue;
            target = slot;
            break;
        }
        if (target == null)
        {
            _counter = 0;
            return;
        }

        if (!Tick()) return;
        foreach (ID outId in GetOutputs(target.ID))
            SpawnOutput(outId, OutputAmount(target.ID));
        target.clear();
        Storage.NotifyChanged();
    }
}

public class CrockPotInfo : ConverterInfo
{
    private static readonly (Dictionary<ID, int> Ingredients, ID Dish)[] Recipes = new[]
    {
        (new Dictionary<ID, int> { { ID.Corn, 1 }, { ID.Egg, 1 } }, ID.StarBun),
        (new Dictionary<ID, int> { { ID.Meat, 1 }, { ID.Pumpkin, 1 }, { ID.Berries, 1 } }, ID.AbyssDelicacy),
        (new Dictionary<ID, int> { { ID.Foul, 1 }, { ID.Meat, 1 }, { ID.Egg, 1 } }, ID.CrimsonSoup),
    };

    protected override int ConvertTime => 120;

    public override bool IsConverting() => MatchRecipe() != null;

    public override void Update()
    {
        if (!CanProcess()) return;
        if (!Tick()) return;

        (Dictionary<ID, int> ingredients, ID dish)? match = MatchRecipe();
        if (match == null) return;

        Dictionary<ID, int> needed = match.Value.ingredients;
        foreach (ItemSlot slot in Storage.List)
        {
            if (slot.isEmpty() || !needed.ContainsKey(slot.ID)) continue;
            int take = Mathf.Min(slot.Stack, needed[slot.ID]);
            needed[slot.ID] -= take;
            slot.Stack -= take;
            if (slot.Stack <= 0) slot.clear();
        }
        SpawnOutput(match.Value.dish);
        Storage.NotifyChanged();
    }

    private (Dictionary<ID, int>, ID)? MatchRecipe()
    {
        var available = new Dictionary<ID, int>();
        foreach (ItemSlot slot in Storage.List)
        {
            if (slot.isEmpty()) continue;
            available[slot.ID] = available.TryGetValue(slot.ID, out int c) ? c + slot.Stack : slot.Stack;
        }

        foreach ((Dictionary<ID, int> ingredients, ID dish) in Recipes)
        {
            bool matches = true;
            foreach ((ID id, int need) in ingredients)
                if (!available.TryGetValue(id, out int have) || have < need) { matches = false; break; }
            if (matches)
                return (new Dictionary<ID, int>(ingredients), dish);
        }
        return null;
    }
}
