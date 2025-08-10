using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Item 
{
    private static readonly Dictionary<string, Item> Dictionary = new Dictionary<string, Item>();

    public static void Initialize()
    {
        AddBlockDefinition("brick", 15, 4, "dig_metal", materials: new Dictionary<string, int> { { "stone", 1 } });
        AddBlockDefinition("marble", 20, 4, "dig_metal", materials: new Dictionary<string, int> { { "stone", 1 }, { "brick", 1 } }, craftStack: 2);
        AddBlockDefinition("dirt", 5, 1, "dig_stone");
        AddBlockDefinition("sand", 5, 1, "dig_sand", materials: new Dictionary<string, int> { { "stone", 1 } }, craftStack: 2);
        AddBlockDefinition("backroom",10,  3, "dig_stone", materials: new Dictionary<string, int> { { "dirt", 1 } }, craftStack: 2);
        AddBlockDefinition("stone",  8, 2, "dig_stone"); 
        AddBlockDefinition("wood", 6, 2, "dig_stone");
        AddBlockDefinition("bullet", 6, 2, "dig_stone", materials: new Dictionary<string, int> { { "dirt", 1 } }, craftStack: 5);

        AddToolDefinition(
            stringID: "sword",
            gesture: ItemGesture.Swing,
            speed: 1.3f,
            range: 1,
            projectileInfo: new SwingProjectileInfo {
                Damage = 25,
                Knockback = 15,
                CritChance = 10,
                Speed = 1.3f,
                Radius = 2f,
            },
            materials: new Dictionary<string, int> { { "stone", 2 }, { "wood", 2 } }
        );

        AddToolDefinition(
            stringID: "axe",
            gesture: ItemGesture.Swing,
            speed: 2f,
            range: 4f,
            projectileInfo: new SwingProjectileInfo {
                Damage = 8,
                Knockback = 10,
                CritChance = 10,
                Speed = 2,
                Radius = 2,
                Mining = 5,
            },
            miningPower: 5,
            materials: new Dictionary<string, int> { { "stone", 1 }, { "wood", 2 } }
        );

        AddToolDefinition(
            stringID: "spear",
            gesture: ItemGesture.Swing,
            speed: 0.3f,
            projectileInfo: new RangedProjectileInfo {
                Sprite = "spear", 
                Damage = 25, 
                Knockback = 10, 
                CritChance = 10, 
                LifeSpan = 10000, 
                Speed = 130, 
                Radius = 0.1f,
                Penetration = 1,
            },
            ammo: "spear",
            materials: new Dictionary<string, int> { { "wood", 1 } },
            holdOutOffset: 1.016f,
            stackSize: 20
        );
        
        AddToolDefinition(
            stringID: "minigun",
            gesture: ItemGesture.Shoot,
            speed: 2f,
            projectileInfo: new RangedProjectileInfo {
                Sprite = "bullet",
                Damage = 10,
                Knockback = 5,
                CritChance = 10,
                LifeSpan = 10000,
                Speed = 50,
                Radius = 0.1f,
                Penetration = 1,
            },
            materials: new Dictionary<string, int> { { "wood", 2 } },
            holdOutOffset: 1.54f
        );

        AddToolDefinition(
            stringID: "pistol",
            gesture: ItemGesture.Shoot,
            speed: 0.6f,
            projectileInfo: new RangedProjectileInfo {
                Sprite = "bullet",
                Damage = 25,
                Knockback = 6,
                CritChance = 10,
                LifeSpan = 10000,
                Speed = 12,
                Radius = 0.1f,
                Penetration = 1
            },
            ammo: "bullet",
            materials: new Dictionary<string, int> { { "wood", 2 } },
            holdOutOffset: 1.016f
        );
    }


    private static void AddBlockDefinition(
        string stringID,
        int breakCost = 1,
        int breakThreshold = 1,
        string sfx = "",
        string name = "",  
        string description = "",
        Dictionary<string, int> materials = null,
        int craftStack = 1,
        int stackSize = 100)
    {
        if (name == "") name = stringID;
        if (sfx == "") sfx = "dig_sand";
        Entity.AddItem(stringID);
        Block.AddBlockDefinition(stringID, breakThreshold, breakCost);

        Item itemData = new Item()
        {
            StringID = stringID,
            StackSize = stackSize,
            Rarity = ItemRarity.Common,

            Scale = 0.6f,
            Sfx = sfx,

            Type = ItemType.Block,
            Gesture = ItemGesture.Swing,

            Speed = 6,
            Range = 5,
            // ProjectileInfo = null,
            // MiningPower = 0,
            // Durability = 0,
            // StatusEffect = null,

            Name = name,
            Description = description
        };

        if (materials != null)
            ItemRecipe.AddRecipe(stringID, materials, craftStack, null);

        Dictionary[stringID] = itemData;
    }

    private static void AddToolDefinition(
        string stringID,
        ItemGesture gesture,
        string sfx = "",
        string name = "",
        int stackSize = 1,
        ItemRarity rarity = ItemRarity.Common,

        float speed = 1,
        float range = 1,
        ProjectileInfo projectileInfo = null,
        string ammo = null,
        int miningPower = 0,
        int durability = 0,
        StatusEffect statusEffect = null,
        float holdOutOffset = 0,

        string description = "",
        Dictionary<string, int> materials = null,
        int craftStack = 1,
        string[] modifiers = null
    )
    {
        if (name == "") name = stringID;
        Entity.AddItem(stringID);

        Item itemData = new Item()
        {
            StringID = stringID,
            StackSize = stackSize,
            Rarity = rarity,

            Scale = 1,
            Sfx = sfx,

            Type = ItemType.Tool,
            Gesture = gesture,
            
            Speed = speed,
            Range = range,
            ProjectileInfo = projectileInfo,
            Ammo = ammo,
            MiningPower = miningPower,
            Durability = durability,
            StatusEffect = statusEffect,
            HoldoutOffset = holdOutOffset,

            Name = name,
            Description = description
        };

        if (materials != null)
            ItemRecipe.AddRecipe(stringID, materials, craftStack, modifiers);

        Dictionary[stringID] = itemData;
    }


    // private static void AddArmorDefinition(string stringID, string name, int stackSize = 1, ItemRarity rarity = ItemRarity.Common,
    //     string description = "", int defense = 0, int speed = 0, int range = 0, Dictionary<string, int> materials = null, int craftStack = 1, string[] modifiers = null)
    // {
    //     Entity.AddItem(stringID);
    //     Item itemData = new Item(stringID, name, stackSize, rarity, description, false, speed, range, null,0, 0, 0,0.8f)
    //     {
    //         Type = ItemType.Armor
    //     };
    //     if (materials != null) Craft.AddCraftingDefinition(stringID, materials, craftStack, modifiers);
    //     Dictionary[stringID] = itemData;
    // }
    //
    // private static void AddAccessoryDefinition(string stringID, string name, int stackSize = 1, ItemRarity rarity = ItemRarity.Common,
    //     string description = "", bool consumable = false, int speed = 0, int range = 0, Dictionary<string, int> materials = null, int craftStack = 1, string[] modifiers = null)
    // {
    //     Entity.AddItem(stringID);
    //     Item itemData = new Item(stringID, name, stackSize, rarity, description, consumable, speed, range, null, 0, 0, 0,0.8f)
    //     {
    //         Type = ItemType.Accessory
    //     };
    //     if (materials != null) Craft.AddCraftingDefinition(stringID, materials, craftStack, modifiers);
    //     Dictionary[stringID] = itemData;
    // }

    // private static void AddFurnitureDefinition(string stringID, string name, int stackSize = 20, ItemRarity rarity = ItemRarity.Common,
    //     string description = "", int speed = 0, int range = 0, Dictionary<string, int> materials = null, int craftStack = 1, string[] modifiers = null)
    // {
    //     Entity.AddItem(stringID);
    //     Item itemData = new Item(stringID, name, stackSize, rarity, description, false, speed, range, null, 0, 0, 0, 1)
    //     {
    //         Type = ItemType.Furniture
    //     };
    //     if (materials != null) Craft.AddCraftingDefinition(stringID, materials, craftStack, modifiers);
    //     Dictionary[stringID] = itemData;
    // }

    public static Item GetItem(string stringID)
    {
        if (Dictionary.ContainsKey(stringID))
        {
            return Dictionary[stringID];
        }
        return null;
    }
}
