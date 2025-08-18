using UnityEngine;
using System.Collections.Generic;
public class LootEntry
{
    public float Chance;
    public float Amount;
    public List<string> ItemIDs;

    public LootEntry(float chance, int amount, params string[] itemIDs)
    {
        Chance = chance;
        Amount = amount;
        ItemIDs = new List<string>(itemIDs);
    }
}

public class Loot
{ 
    private static readonly Dictionary<string, Loot> Dictionary = new Dictionary<string, Loot>();
    private readonly List<LootEntry> _table = new List<LootEntry>();
    public static void Initialize()
    {
        Loot loot = CreateTable("chest");
        loot.Add(1, 1, "stone");
        loot.Add(0.5f, 2, "stone");
        loot.Add(1, 7, "brick"); 
        loot.Add(0.5f, 2, "brick");
        loot.Add(0.5f, 5, "marble");  
        loot.Add(1, 1, "sword", "axe_diamond"); 
        loot.Add(0.5f, 1, "minigun"); 
        
        loot = CreateTable("slab");
        loot.Add(1, 3, "stone"); 
        loot.Add(0.5f, 1, "stone");
        loot.Add(0.5f, 1, "marble");
        loot.Add(0.5f, 1, "marble", "sand"); 
        
        loot = CreateTable("tree");
        loot.Add(1, 3, "wood");
        loot.Add(0.5f, 2, "wood"); 
        
        loot = CreateTable("megumin");
        loot.Add(0.1f, 1, "sword", "axe_diamond"); 
        
        loot = CreateTable("chito");
        loot.Add(0.7f, 10, "bullet"); 
        loot.Add(0.1f, 1, "pistol"); 
        
        loot = CreateTable("snare_flea");
        loot.Add(0.5f, 6, "wood"); 
        
        loot = CreateTable("yuuri");
        loot.Add(0.7f, 10, "bullet");  
        loot.Add(0.1f, 1, "pistol"); 
        
        loot = CreateTable("stone");
        loot.Add(1, 3, "gravel");  
        loot.Add(0.5f, 1, "gravel"); 
        loot.Add(0.5f, 1, "gravel"); 
    }

    public static Loot CreateTable(string id)
    {
        Loot loot = new Loot();
        Dictionary.Add(id, loot);
        return loot;
    }
    
    public static Loot Gettable(string id)
    {
        return Dictionary[id];
    }
     
    public void Add(float chance, int amount, params string[] itemIDs)
    {
        _table.Add(new LootEntry(chance, amount, itemIDs));
    }

    public void Spawn(Vector3 position)
    {
        Vector3Int worldPosition = Vector3Int.FloorToInt(position);

        foreach (var entry in _table)
        {
            List<string> items = entry.ItemIDs;

            for (int i = 0; i <  entry.Amount; i++)
            {
                if (Random.value <= entry.Chance)
                {
                    string itemID = items[Random.Range(0, items.Count)];
                    Entity.SpawnItem(itemID, worldPosition);
                }
            }
        }
    }
    
    public void AddToContainer(Storage storage)
    { 
        foreach (var entry in _table)
        {
            List<string> items = entry.ItemIDs;
            for (int i = 0; i < entry.Amount; i++)
            {
                if (Random.value <= entry.Chance)
                {
                    string itemID = items[Random.Range(0, items.Count)];
                    storage.AddItem(itemID);
                }
            } 
        }
    }
}