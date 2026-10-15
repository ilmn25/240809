using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class ConverterInfo : ContainerInfo
{
    protected virtual IReadOnlyList<ID> GetOutputs(ID input) => null;
    protected virtual int OutputAmount(ID input) => 1;
    protected virtual int ConvertTime => 90;

    /// <summary>True when this converter takes the item. Defaults to having outputs.</summary>
    protected virtual bool Accepts(ID input) => GetOutputs(input) != null;

    /// <summary>Pays out one converted input; the input slot is cleared afterwards.</summary>
    protected virtual void PayOut(ID input)
    {
        foreach (ID outId in GetOutputs(input))
            SpawnOutput(outId, OutputAmount(input));
    }

    [NonSerialized] private int _counter;

    public virtual bool IsConverting()
    {
        if (Storage == null || Storage.List == null) return false;
        foreach (ItemSlot slot in Storage.List)
        {
            if (slot.isEmpty()) continue;
            if (Accepts(slot.ID)) return true;
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
            if (Accepts(slot.ID)) continue;
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
        PayOut(target.ID);
        target.clear();
        Storage.NotifyChanged();
    }
}

/// <summary>A converter that consumes a batch of ingredients from its storage and
/// produces one output: the crock pot's dishes.</summary>
public abstract class BatchConverterInfo : ConverterInfo
{
    /// <summary>The batch this converter can run right now, or null when it can't.</summary>
    protected abstract (Dictionary<ID, int> Ingredients, ID Output)? MatchBatch();

    public override bool IsConverting() => Storage?.List != null && MatchBatch() != null;

    public override void Update()
    {
        if (!CanProcess()) return;
        if (!Tick()) return;

        (Dictionary<ID, int> ingredients, ID output)? match = MatchBatch();
        if (match == null) return;

        Consume(match.Value.ingredients);
        SpawnOutput(match.Value.output);
        Storage.NotifyChanged();
    }

    /// <summary>Takes the matched ingredients out of the storage, pulling from as many
    /// slots as the batch needs.</summary>
    private void Consume(Dictionary<ID, int> needed)
    {
        foreach (ItemSlot slot in Storage.List)
        {
            if (slot.isEmpty() || !needed.TryGetValue(slot.ID, out int need)) continue;
            int take = Mathf.Min(slot.Stack, need);
            needed[slot.ID] = need - take;
            slot.Stack -= take;
            if (slot.Stack <= 0) slot.clear();
        }
    }
}

public class CrockPotInfo : BatchConverterInfo
{
    private static readonly (Dictionary<ID, int> Ingredients, ID Dish)[] Recipes = new[]
    {
        (new Dictionary<ID, int> { { ID.Corn, 1 }, { ID.Egg, 1 } }, ID.StarBun),
        (new Dictionary<ID, int> { { ID.Meat, 1 }, { ID.Pumpkin, 1 }, { ID.Berries, 1 } }, ID.AbyssDelicacy),
        (new Dictionary<ID, int> { { ID.Foul, 1 }, { ID.Meat, 1 }, { ID.Egg, 1 } }, ID.CrimsonSoup),
    };

    protected override int ConvertTime => 120;

    protected override (Dictionary<ID, int>, ID)? MatchBatch()
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
