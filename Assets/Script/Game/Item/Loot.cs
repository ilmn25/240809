using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class Loot
{ 
    private static readonly Dictionary<string, Loot> Dictionary = new Dictionary<string, Loot>();
    public static void Initialize()
    {
        Loot loot = CreateTable("slab");
        loot.Add(1, "stone");
        loot.Add(1, "stone");
        loot.Add(1, "stone");
        loot.Add(0.5f, "stone");
        loot.Add(0.5f, "marble");
        loot.Add(0.5f, "marble", "sand"); 
        
        loot = CreateTable("tree");
        loot.Add(1, "wood");
        loot.Add(1, "wood");
        loot.Add(1, "wood");
        loot.Add(0.5f, "wood");
        loot.Add(0.5f, "wood");
        
        loot = CreateTable("megumin");
        loot.Add(1, "marble"); 
        loot = CreateTable("chito");
        loot.Add(1, "marble"); 
        loot = CreateTable("snare_flea");
        loot.Add(1, "marble"); 
        loot = CreateTable("yuuri");
        loot.Add(1, "marble"); 
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
    
    public readonly Dictionary<float, List<string>> Table = new Dictionary<float, List<string>>();
    public void Add(float chance, params string[] itemIDs)
    {
        if (!Table.ContainsKey(chance))
        {
            Table[chance] = new List<string>();
        }
        Table[chance].AddRange(itemIDs);
    }

    public void Spawn(Vector3 position)
    {
        Vector3Int worldPosition = Vector3Int.FloorToInt(position);

        foreach (var entry in Table)
        {
            float chance = entry.Key;
            List<string> items = entry.Value;

            if (Random.value <= chance && items.Count > 0)
            {
                string itemID = items[Random.Range(0, items.Count)];
                Entity.SpawnItem(itemID, worldPosition);
            }
        }
    }
 
}