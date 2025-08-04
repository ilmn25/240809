using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Item 
{
    private static readonly Dictionary<string, Item> Dictionary = new Dictionary<string, Item>();

    public static void Initialize()
    {
        AddBlockDefinition("brick", breakCost: 15, breakThreshold: 4, materials: new Dictionary<string, int> { { "stone", 1 } });
        AddBlockDefinition("marble", breakCost: 20, breakThreshold: 4, materials: new Dictionary<string, int> { { "stone", 1 }, { "brick", 1 } }, craftStack: 2);
        AddBlockDefinition("dirt", breakCost: 5, breakThreshold: 1);
        AddBlockDefinition("sand", breakCost: 5, breakThreshold: 1, materials: new Dictionary<string, int> { { "stone", 1 } }, craftStack: 2);  
        AddBlockDefinition("backroom", breakCost: 10, breakThreshold: 3, materials: new Dictionary<string, int> { { "dirt", 1 } }, craftStack: 2);
        AddBlockDefinition("stone", breakCost: 8, breakThreshold: 2);
        AddBlockDefinition("wood", breakCost: 6, breakThreshold: 2);

        AddToolDefinition(
            stringID: "sword",
            gesture: ItemGesture.Swing,
            speed: 1.1f,
            range: 1.5f,
            projectileInfo: new SwingProjectileInfo(10, 10, 10, 1.1f, 1.5f),
            miningPower: 0,
            materials: new Dictionary<string, int> { { "stone", 2 }, { "wood", 2 } }
        );

        AddToolDefinition(
            stringID: "axe",
            gesture: ItemGesture.Swing, 
            speed: 0.7f,
            range: 4f,
            projectileInfo: new SwingProjectileInfo(3, 10, 10, 1.1f, 1.5f),
            miningPower: 5,
            materials: new Dictionary<string, int> { { "stone", 1 }, { "wood", 2 } }
        );
    }

    private static void AddBlockDefinition(string stringID, string name = "", int breakCost = 1, int breakThreshold = 1, string description = "", Dictionary<string, int> materials = null, int craftStack = 1,
        string[] modifiers = null, int stackSize = 100, ItemRarity rarity = ItemRarity.Common)
    {
        if (name == "") name = stringID;
        Entity.AddItem(stringID);
        Block.AddBlockDefinition(stringID, breakThreshold, breakCost);
        Item itemData = new Item(stringID, name, stackSize, rarity, description, false,  2, 5, null,0, 0, 0)
        {
            Type = ItemType.Block,
            Gesture = ItemGesture.Swing
        };
        if (materials != null) Craft.AddCraftingDefinition(stringID, materials, craftStack, modifiers);
        Dictionary[stringID] = itemData;
    }

    private static void AddToolDefinition(string stringID, ItemGesture gesture, string name = "", float speed = 0,
        float range = 1, ProjectileInfo projectileInfo = null, int miningPower = 0, Dictionary<string, int> materials = null, string description = "", int craftStack = 1,
        string[] modifiers = null, int stackSize = 1, ItemRarity rarity = ItemRarity.Common, bool consumable = false,
        int healHp = 0, int healMana = 0)
    {
        if (name == "") name = stringID;
        Entity.AddItem(stringID);
        Item itemData = new Item(stringID, name, stackSize, rarity, description, consumable, speed, range, projectileInfo, miningPower, healHp, healMana)
        {
            Type = ItemType.Tool,
            Gesture = gesture
        };
        if (materials != null) Craft.AddCraftingDefinition(stringID, materials, craftStack, modifiers);
        Dictionary[stringID] = itemData;
    }

    private static void AddArmorDefinition(string stringID, string name, int stackSize = 1, ItemRarity rarity = ItemRarity.Common,
        string description = "", int defense = 0, int speed = 0, int range = 0, Dictionary<string, int> materials = null, int craftStack = 1, string[] modifiers = null)
    {
        Entity.AddItem(stringID);
        Item itemData = new Item(stringID, name, stackSize, rarity, description, false, speed, range, null,0, 0, 0)
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
        Item itemData = new Item(stringID, name, stackSize, rarity, description, consumable, speed, range, null, 0, 0, 0)
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
        Item itemData = new Item(stringID, name, stackSize, rarity, description, false, speed, range, null, 0, 0, 0)
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
