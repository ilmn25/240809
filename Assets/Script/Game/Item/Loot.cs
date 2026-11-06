using UnityEngine;
using System.Collections.Generic;
public class LootEntry
{
    public float Chance;
    public float Amount;
    public List<ID> ItemIDs;

    public LootEntry(float chance, int amount, params ID[] itemIDs)
    {
        Chance = chance;
        Amount = amount;
        ItemIDs = new List<ID>(itemIDs);
    }
}

public class Loot
{ 
    private static readonly Dictionary<ID, Loot> Dictionary = new Dictionary<ID, Loot>();
    private readonly List<LootEntry> _table = new List<LootEntry>(); 
    public Loot(ID id)
    {
        // One table per id: break/burn/mob/harvest paths all look loot up by
        // id, so each entity registers its single table exactly once. Adding a
        // second table for the same id throws here on purpose to catch mistakes.
        Dictionary.Add(id, this);
    }

    public static Loot Gettable(ID id)
    {
        return Dictionary[id];
    }

    public static bool TryGet(ID id, out Loot loot)
    {
        return Dictionary.TryGetValue(id, out loot);
    }
     
    public void Add(float chance, int amount, params ID[] itemIDs)
    {
        _table.Add(new LootEntry(chance, amount, itemIDs));
    }

    /// <summary>Roll the whole table, yielding one item id per successful chance roll.
    /// The single place loot is rolled — every drop path consumes this.</summary>
    private IEnumerable<ID> Roll()
    {
        foreach (LootEntry entry in _table)
        {
            for (int i = 0; i < entry.Amount; i++)
            {
                if (Random.value > entry.Chance) continue;
                yield return entry.ItemIDs[Random.Range(0, entry.ItemIDs.Count)];
            }
        }
    }

    public void Spawn(Vector3 position)
    {
        foreach (ID itemID in Roll())
            Entity.SpawnItem(itemID, position, stackOnSpawn: false);
    }

    public void SpawnOneRandom(Vector3 position)
    {
        if (_table.Count == 0) return;

        LootEntry entry = _table[Random.Range(0, _table.Count)];
        ID itemID = entry.ItemIDs[Random.Range(0, entry.ItemIDs.Count)];
        Entity.SpawnItem(itemID, position, stackOnSpawn: false);
    }

    /// <summary>Roll the loot table at the burn site, converting any burnable
    /// item (wood, plants) to its burn result (e.g. charcoal).</summary>
    public void SpawnBurned(Vector3 position)
    {
        foreach (ID itemID in Roll())
            Entity.SpawnItem(BurnUtil.BurnResultOf(itemID), position, stackOnSpawn: false);
    }

    public void AddToContainer(Storage storage)
    {
        foreach (ID itemID in Roll())
            storage.CreateAndAddItem(itemID);
    }
}