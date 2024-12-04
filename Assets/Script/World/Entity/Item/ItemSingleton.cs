using System;
using System.Collections;
using System.Collections.Generic;
using Script.World.Entity.Item;
using UnityEngine;

public class ItemSingleton : MonoBehaviour
{
    public static ItemSingleton Instance { get; private set; }  
    private static Dictionary<string, ItemData> _itemList  = new Dictionary<string, ItemData>();
 
    public void Awake()
    {
        Instance = this;
        
        AddBlockDefinition("brick", "Brick", 20, ItemRarity.Common, "A basic brick.",
            2, 4,
            new Dictionary<string, int> { 
                { "stone", 1 } 
            });
        
        AddBlockDefinition("marble", "Marble", 20, ItemRarity.Common, "A basic marble.",
            2 ,4,
            new Dictionary<string, int> { 
                { "stone", 1 }, 
                { "brick", 1 } 
            }, 2);
        
        AddBlockDefinition("dirt", "Dirt", 20, ItemRarity.Common, "A basic dirt.",
            1, 1);
        
        AddBlockDefinition("sand", "Sand", 20, ItemRarity.Common, "some sand.",
            1, 1,
            new Dictionary<string, int> { 
                { "stone", 1 } 
            }, 2);  
        
        AddBlockDefinition("backroom", "Backroom", 20, ItemRarity.Common, "A basic backroom.",
            2, 3, 
            new Dictionary<string, int> { 
                { "dirt", 1 } 
            }, 2);
        
        AddBlockDefinition("stone", "Stone", 20, ItemRarity.Common, "A basic stone.",
            1, 2);
        
        AddBlockDefinition("wood", "Wood", 20, ItemRarity.Common, "A basic wood.",
            1, 2);
        
        AddToolDefinition("sword", "Sword", 1, ItemRarity.Common, "A basic sword.",
            false, 10, 2, 20,
            new Dictionary<string, int> { 
                { "stone", 2 }, 
                { "wood", 2 } 
            });
        
        AddToolDefinition("axe", "Axe", 1, ItemRarity.Common, "A basic sword.",
            false, 10, 2, 20, 
            new Dictionary<string, int> { 
                { "stone", 1 }, 
                { "wood", 2 } 
            });
    }

    private static void AddBlockDefinition(string stringID, string name, int stackSize = 20, ItemRarity rarity = ItemRarity.Common,
        string description = "", int breakCost = 1, int breakThreshold = 1, Dictionary<string, int> materials = null, int craftStack = 1, string[] modifiers = null)
    {
        BlockSingleton.AddBlockDefinition(stringID, breakCost, breakThreshold);
        ItemData itemData = new ItemData(stringID, name, stackSize, rarity, description, false, 0, 0, 0)
        {
            Type = ItemType.Block
        };
        if (materials != null) CraftSingleton.AddCraftingDefinition(stringID, materials, craftStack, modifiers);
        _itemList[stringID] = itemData;
    }

    private static void AddToolDefinition(string stringID, string name, int stackSize = 1, ItemRarity rarity = ItemRarity.Common,
        string description = "", bool consumable = false, int damage = 0, float knockback = 0, int useTime = 0, Dictionary<string, int> materials = null, int craftStack = 1, string[] modifiers = null)
    {
        ItemData itemData = new ItemData(stringID, name, stackSize, rarity, description, consumable, damage, knockback, useTime)
        {
            Type = ItemType.Tool
        };
        if (materials != null) CraftSingleton.AddCraftingDefinition(stringID, materials, craftStack, modifiers);
        _itemList[stringID] = itemData;
    }

    private static void AddArmorDefinition(string stringID, string name, int stackSize = 1, ItemRarity rarity = ItemRarity.Common,
        string description = "", int defense = 0, Dictionary<string, int> materials = null, int craftStack = 1, string[] modifiers = null)
    {
        ItemData itemData = new ItemData(stringID, name, stackSize, rarity, description, false, 0, 0, 0)
        {
            Type = ItemType.Armor,
            // Add additional armor-specific properties here if needed
        };
        if (materials != null) CraftSingleton.AddCraftingDefinition(stringID, materials, craftStack, modifiers);
        _itemList[stringID] = itemData;
    }

    private static void AddAccessoryDefinition(string stringID, string name, int stackSize = 1, ItemRarity rarity = ItemRarity.Common,
        string description = "", bool consumable = false, Dictionary<string, int> materials = null, int craftStack = 1, string[] modifiers = null)
    {
        ItemData itemData = new ItemData(stringID, name, stackSize, rarity, description, consumable, 0, 0, 0)
        {
            Type = ItemType.Accessory
        };
        if (materials != null) CraftSingleton.AddCraftingDefinition(stringID, materials, craftStack, modifiers);
        _itemList[stringID] = itemData;
    }

    private static void AddFurnitureDefinition(string stringID, string name, int stackSize = 20, ItemRarity rarity = ItemRarity.Common,
        string description = "", Dictionary<string, int> materials = null, int craftStack = 1, string[] modifiers = null)
    {
        ItemData itemData = new ItemData(stringID, name, stackSize, rarity, description, false, 0, 0, 0)
        {
            Type = ItemType.Furniture
        };
        if (materials != null) CraftSingleton.AddCraftingDefinition(stringID, materials, craftStack, modifiers);
        _itemList[stringID] = itemData;
    }
 

    public static ItemData GetItem(string stringID)
    {
        if (_itemList.ContainsKey(stringID))
        {
            return _itemList[stringID];
        }
        return null;
    }
  
}