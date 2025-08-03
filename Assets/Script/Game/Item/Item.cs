using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Item 
{
    private static Dictionary<string, Item> Dictionary = new Dictionary<string, Item>();

    public static void Initialize()
    {
        AddBlockDefinition("brick", "Brick", breakCost: 15, breakThreshold: 4, speed: 0, range: 0, materials: new Dictionary<string, int> { { "stone", 1 } });
        AddBlockDefinition("marble", "Marble", breakCost: 20, breakThreshold: 4, speed: 0, range: 0, materials: new Dictionary<string, int> { { "stone", 1 }, { "brick", 1 } }, craftStack: 2);
        AddBlockDefinition("dirt", "Dirt", breakCost: 5, breakThreshold: 1, speed: 0, range: 0);
        AddBlockDefinition("sand", "Sand", breakCost: 5, breakThreshold: 1, speed: 0, range: 0, materials: new Dictionary<string, int> { { "stone", 1 } }, craftStack: 2);  
        AddBlockDefinition("backroom", "Backroom", breakCost: 10, breakThreshold: 3, speed: 0, range: 0, materials: new Dictionary<string, int> { { "dirt", 1 } }, craftStack: 2);
        AddBlockDefinition("stone", "Stone", breakCost: 8, breakThreshold: 2, speed: 0, range: 0, description: "A basic stone.");
        AddBlockDefinition("wood", "Wood", breakCost: 6, breakThreshold: 2, speed: 0, range: 0, description: "A basic wood.");

        AddToolDefinition("sword", "Sword", damage: 10, knockback: 10, critChance: 5, speed: 1.1f, range: 1.5f, miningPower: 0, materials: new Dictionary<string, int> { { "stone", 2 }, { "wood", 2 } });
        AddToolDefinition("axe", "Axe", damage: 6, knockback: 10, critChance: 3, speed: 0.7f, range: 1.5f, miningPower: 5, materials: new Dictionary<string, int> { { "stone", 1 }, { "wood", 2 } });
    }

    private static void AddBlockDefinition(string stringID, string name, int breakCost = 1, int breakThreshold = 1,
        float speed = 0, float range = 0, string description = "", Dictionary<string, int> materials = null, int craftStack = 1,
        string[] modifiers = null, int stackSize = 100, ItemRarity rarity = ItemRarity.Common)
    {
        Entity.AddItem(stringID);
        Block.AddBlockDefinition(stringID, breakThreshold, breakCost);
        Item itemData = new Item(stringID, name, stackSize, rarity, description, false, 0, 0, 0, speed, range, 0, 0, 0)
        {
            Type = ItemType.Block
        };
        if (materials != null) Craft.AddCraftingDefinition(stringID, materials, craftStack, modifiers);
        Dictionary[stringID] = itemData;
    }

    private static void AddToolDefinition(string stringID, string name, int damage = 0, float knockback = 0, int critChance = 0, float speed = 0,
        float range = 1, int miningPower = 0, Dictionary<string, int> materials = null, string description = "", int craftStack = 1,
        string[] modifiers = null, int stackSize = 1, ItemRarity rarity = ItemRarity.Common, bool consumable = false,
        int healHP = 0, int healMana = 0)
    {
        Entity.AddItem(stringID);
        Item itemData = new Item(stringID, name, stackSize, rarity, description, consumable, damage, knockback, critChance, speed, range, miningPower, healHP, healMana)
        {
            Type = ItemType.Tool
        };
        if (materials != null) Craft.AddCraftingDefinition(stringID, materials, craftStack, modifiers);
        Dictionary[stringID] = itemData;
    }

    private static void AddArmorDefinition(string stringID, string name, int stackSize = 1, ItemRarity rarity = ItemRarity.Common,
        string description = "", int defense = 0, int speed = 0, int range = 0, Dictionary<string, int> materials = null, int craftStack = 1, string[] modifiers = null)
    {
        Entity.AddItem(stringID);
        Item itemData = new Item(stringID, name, stackSize, rarity, description, false, 0, 0, 0, speed, range, 0, 0, 0)
        {
            Type = ItemType.Armor
        };
        if (materials != null) Craft.AddCraftingDefinition(stringID, materials, craftStack, modifiers);
        Dictionary[stringID] = itemData;
    }

    private static void AddAccessoryDefinition(string stringID, string name, int stackSize = 1, ItemRarity rarity = ItemRarity.Common,
        string description = "", bool consumable = false, int speed = 0, int range = 0, Dictionary<string, int> materials = null, int craftStack = 1, string[] modifiers = null)
    {
        Entity.AddItem(stringID);
        Item itemData = new Item(stringID, name, stackSize, rarity, description, consumable, 0, 0, 0, speed, range, 0, 0, 0)
        {
            Type = ItemType.Accessory
        };
        if (materials != null) Craft.AddCraftingDefinition(stringID, materials, craftStack, modifiers);
        Dictionary[stringID] = itemData;
    }

    private static void AddFurnitureDefinition(string stringID, string name, int stackSize = 20, ItemRarity rarity = ItemRarity.Common,
        string description = "", int speed = 0, int range = 0, Dictionary<string, int> materials = null, int craftStack = 1, string[] modifiers = null)
    {
        Entity.AddItem(stringID);
        Item itemData = new Item(stringID, name, stackSize, rarity, description, false, 0, 0, 0, speed, range, 0, 0, 0)
        {
            Type = ItemType.Furniture
        };
        if (materials != null) Craft.AddCraftingDefinition(stringID, materials, craftStack, modifiers);
        Dictionary[stringID] = itemData;
    }

    public static Item GetItem(string stringID)
    {
        if (Dictionary.ContainsKey(stringID))
        {
            return Dictionary[stringID];
        }
        return null;
    }
}
