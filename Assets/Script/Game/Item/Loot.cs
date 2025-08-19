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
    public static void Initialize()
    {
        Loot loot = CreateTable(ID.Chest);
        loot.Add(1, 1, ID.StoneBlock);
        loot.Add(0.5f, 2, ID.StoneBlock);
        loot.Add(1, 7, ID.Brick); 
        loot.Add(0.5f, 2, ID.Brick);
        loot.Add(0.5f, 5, ID.MarbleBlock);  
        loot.Add(1, 1, ID.Sword, ID.DiamondAxe); 
        loot.Add(0.5f, 1, ID.Minigun); 
        
        loot = CreateTable(ID.Slab);
        loot.Add(1, 3, ID.StoneBlock); 
        loot.Add(0.5f, 1, ID.StoneBlock);
        loot.Add(0.5f, 1, ID.MarbleBlock);
        loot.Add(0.5f, 1, ID.MarbleBlock, ID.SandBlock); 
        
        loot = CreateTable(ID.Tree);
        loot.Add(1, 3, ID.WoodBlock);
        loot.Add(0.5f, 2, ID.WoodBlock); 
        
        loot = CreateTable(ID.Megumin);
        loot.Add(0.1f, 1, ID.Sword, ID.DiamondAxe); 
        
        loot = CreateTable(ID.Chito);
        loot.Add(0.7f, 10, ID.Bullet); 
        loot.Add(0.1f, 1, ID.Pistol); 
        
        loot = CreateTable(ID.SnareFlea);
        loot.Add(0.5f, 6, ID.WoodBlock); 
        
        loot = CreateTable(ID.Yuuri);
        loot.Add(0.7f, 10, ID.Bullet);  
        loot.Add(0.1f, 1, ID.Pistol); 
         
    }

    public static Loot CreateTable(ID id)
    {
        Loot loot = new Loot();
        Dictionary.Add(id, loot);
        return loot;
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
                    storage.AddItem(itemID);
                }
            } 
        }
    }
}