using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item 
{
    private static Dictionary<string, ItemData> Dictionary  = new Dictionary<string, ItemData>();
    
    public static void Initialize()
    {
        AddBlockDefinition("brick", "Brick",
            breakCost: 2, breakThreshold: 4, materials: new Dictionary<string, int> { { "stone", 1 } });
        
        AddBlockDefinition("marble", "Marble",
            breakCost: 2 ,breakThreshold: 4, materials: new Dictionary<string, int> { { "stone", 1 }, { "brick", 1 } },
            craftStack: 2);
        
        AddBlockDefinition("dirt", "Dirt",
            breakCost: 1, breakThreshold: 1);
        
        AddBlockDefinition("sand", "Sand",
            breakCost: 1, breakThreshold: 1, materials: new Dictionary<string, int> { { "stone", 1 } }, craftStack: 2);  
        
        AddBlockDefinition("backroom", "Backroom",
            breakCost: 2, breakThreshold: 3, materials: new Dictionary<string, int> { { "dirt", 1 } }, craftStack: 2);
        
        AddBlockDefinition("stone", "Stone",
            breakCost: 1, breakThreshold: 2, description: "A basic stone.");
        
        AddBlockDefinition("wood", "Wood",
            breakCost: 1, breakThreshold: 2, description: "A basic wood.");
         
        AddToolDefinition("sword", "Sword", 10, 2, 20,
            new Dictionary<string, int> { { "stone", 2 }, { "wood", 2 } });
        
        AddToolDefinition("axe", "Axe", 10, 2, 20,
            new Dictionary<string, int> { { "stone", 1 }, { "wood", 2 } });
            
    }

    private static void AddBlockDefinition(string stringID, string name, int breakCost = 1, int breakThreshold = 1,
        string description = "", Dictionary<string, int> materials = null, int craftStack = 1,
        string[] modifiers = null, int stackSize = 100, ItemRarity rarity = ItemRarity.Common)
    {
        Entity.AddItem(stringID);
        Block.AddBlockDefinition(stringID, breakCost, breakThreshold);
        ItemData itemData = new ItemData(stringID, name, stackSize, rarity, description, false, 0, 0, 0)
        {
            Type = ItemType.Block
        };
        if (materials != null) Craft.AddCraftingDefinition(stringID, materials, craftStack, modifiers);
        Dictionary[stringID] = itemData;
    }

    private static void AddToolDefinition(string stringID, string name, int damage = 0, float knockback = 0, int useTime = 0, Dictionary<string, int> materials = null, string description = "", int craftStack = 1, string[] modifiers = null, int stackSize = 1, ItemRarity rarity = ItemRarity.Common,
    bool consumable = false)
    {
        Entity.AddItem(stringID);
        ItemData itemData = new ItemData(stringID, name, stackSize, rarity, description, consumable, damage, knockback, useTime)
        {
            Type = ItemType.Tool
        };
        if (materials != null) Craft.AddCraftingDefinition(stringID, materials, craftStack, modifiers);
        Dictionary[stringID] = itemData;
    }

    private static void AddArmorDefinition(string stringID, string name, int stackSize = 1, ItemRarity rarity = ItemRarity.Common,
        string description = "", int defense = 0, Dictionary<string, int> materials = null, int craftStack = 1, string[] modifiers = null)
    {
        Entity.AddItem(stringID);
        ItemData itemData = new ItemData(stringID, name, stackSize, rarity, description, false, 0, 0, 0)
        {
            Type = ItemType.Armor,
            // Add additional armor-specific properties here if needed
        };
        if (materials != null) Craft.AddCraftingDefinition(stringID, materials, craftStack, modifiers);
        Dictionary[stringID] = itemData;
    }

    private static void AddAccessoryDefinition(string stringID, string name, int stackSize = 1, ItemRarity rarity = ItemRarity.Common,
        string description = "", bool consumable = false, Dictionary<string, int> materials = null, int craftStack = 1, string[] modifiers = null)
    {
        Entity.AddItem(stringID);
        ItemData itemData = new ItemData(stringID, name, stackSize, rarity, description, consumable, 0, 0, 0)
        {
            Type = ItemType.Accessory
        };
        if (materials != null) Craft.AddCraftingDefinition(stringID, materials, craftStack, modifiers);
        Dictionary[stringID] = itemData;
    }

    private static void AddFurnitureDefinition(string stringID, string name, int stackSize = 20, ItemRarity rarity = ItemRarity.Common,
        string description = "", Dictionary<string, int> materials = null, int craftStack = 1, string[] modifiers = null)
    {
        Entity.AddItem(stringID);
        ItemData itemData = new ItemData(stringID, name, stackSize, rarity, description, false, 0, 0, 0)
        {
            Type = ItemType.Furniture
        };
        if (materials != null) Craft.AddCraftingDefinition(stringID, materials, craftStack, modifiers);
        Dictionary[stringID] = itemData;
    }
 

    public static ItemData GetItem(string stringID)
    {
        if (Dictionary.ContainsKey(stringID))
        {
            return Dictionary[stringID];
        }
        return null;
    }
  
}