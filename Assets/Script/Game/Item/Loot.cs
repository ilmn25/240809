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
        Dictionary.Add(id, this);
    }
    
    public static Loot Gettable(ID id)
    {
        return Dictionary[id];
    }
     
    public void Add(float chance, int amount, params ID[] itemIDs)
    {
        _table.Add(new LootEntry(chance, amount, itemIDs));
    }

    public void Spawn(Vector3 position)
    {
        Vector3Int worldPosition = Vector3Int.FloorToInt(position);

        foreach (var entry in _table)
        {
            List<ID> items = entry.ItemIDs;

            for (int i = 0; i <  entry.Amount; i++)
            {
                if (Random.value <= entry.Chance)
                {
                    ID itemID = items[Random.Range(0, items.Count)];
                    Entity.SpawnItem(itemID, worldPosition);
                }
            }
        }
    }
    
    public void AddToContainer(Storage storage)
    { 
        foreach (var entry in _table)
        {
            List<ID> items = entry.ItemIDs;
            for (int i = 0; i < entry.Amount; i++)
            {
                if (Random.value <= entry.Chance)
                {
                    ID itemID = items[Random.Range(0, items.Count)];
                    storage.CreateAndAddItem(itemID);
                }
            } 
        }
    }
}