using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Item 
{
    private static readonly Dictionary<ID, Item> Dictionary = new ();
    public static void Initialize()
    {
        Loot loot;

        // Blocks
        AddBlockDefinition(ID.BrickBlock, 70, 3, SfxID.HitMetal, materials: new Dictionary<ID, int> { { ID.Gravel, 3 } });
        loot = Loot.CreateTable(ID.BrickBlock);
        loot.Add(1, 3, ID.Gravel);
        loot.Add(0.5f, 1, ID.Slag);
        loot.Add(0.5f, 1, ID.Brick);
        AddBlockDefinition(ID.MarbleBlock, 100, 3, SfxID.HitMetal, materials: new Dictionary<ID, int> { { ID.StoneBlock, 1 }, { ID.BrickBlock, 1 } }, craftStack: 2);
        AddBlockDefinition(ID.DirtBlock, 50, 1, SfxID.HitStone);
        loot = Loot.CreateTable(ID.DirtBlock);
        loot.Add(1, 3, ID.Gravel);
        loot.Add(0.5f, 1, ID.Flint);
        loot.Add(0.5f, 1, ID.Sticks);
        AddBlockDefinition(ID.SandBlock, 40, 1, SfxID.HitSand, materials: new Dictionary<ID, int> { { ID.StoneBlock, 1 } }, craftStack: 2);
        AddBlockDefinition(ID.BackroomBlock, 100, 3, SfxID.HitStone, materials: new Dictionary<ID, int> { { ID.DirtBlock, 1 } }, craftStack: 2);
        AddBlockDefinition(ID.StoneBlock, 100, 2, SfxID.HitStone);
        loot = Loot.CreateTable(ID.StoneBlock);
        loot.Add(1, 3, ID.Gravel);
        loot.Add(0.5f, 1, ID.MetalChunks);
        loot.Add(0.5f, 1, ID.MetalChunks);
        AddBlockDefinition(ID.WoodBlock, 100, 2, SfxID.HitStone);
        AddBlockDefinition(ID.GraniteBlock, 100, 2, SfxID.HitStone);

        // Materials
        AddMaterialDefinition(ID.Bullet, materials: new Dictionary<ID, int> { { ID.Gravel, 2 } }, craftStack: 5, time:1500);
        AddMaterialDefinition(ID.Gravel);
        AddMaterialDefinition(ID.Sticks);
        AddMaterialDefinition(ID.Flint);
        AddMaterialDefinition(ID.MetalChunks);
        AddMaterialDefinition(ID.Charcoal, materials: new Dictionary<ID, int> { { ID.Log, 2 } }, time:1500);
        AddMaterialDefinition(ID.Steel, materials: new Dictionary<ID, int> { { ID.MetalChunks, 3 }, {ID.Charcoal, 2} }, time:2000);
        AddMaterialDefinition(ID.Brick, materials: new Dictionary<ID, int> { { ID.Slag, 3 } }, time:1500);
        AddMaterialDefinition(ID.Stake, materials: new Dictionary<ID, int> { { ID.Log, 3 } });
        AddMaterialDefinition(ID.Slag, materials: new Dictionary<ID, int> { { ID.Gravel, 3 }, {ID.Charcoal, 2} }, time:1500);
        AddMaterialDefinition(ID.Log);
        AddMaterialDefinition(ID.Plank, materials: new Dictionary<ID, int> { { ID.Log, 3 } }, time:1500);

        // Structures
        AddStructureDefinition(ID.Chest, new Dictionary<ID, int> { { ID.Plank, 5 } }, 100);
        AddStructureDefinition(ID.Station, new Dictionary<ID, int> { { ID.Log, 15 }, { ID.Flint, 5 } }, 100);
        AddStructureDefinition(ID.Workbench, new Dictionary<ID, int> { { ID.Log, 15 } }, 100);
        AddStructureDefinition(ID.Stonecutter, new Dictionary<ID, int> { { ID.Steel, 2 }, { ID.Slag, 6 }, { ID.Plank, 5 }}, 100);
        AddStructureDefinition(ID.Sawmill, new Dictionary<ID, int> { { ID.Steel, 1 }, { ID.Slag, 6 }, { ID.Log, 5 }}, 100);
        AddStructureDefinition(ID.Campfire, new Dictionary<ID, int> { { ID.Gravel, 4 }, { ID.Flint, 1 }, { ID.Log, 15 }}, 100);
        AddStructureDefinition(ID.Furnace, new Dictionary<ID, int> { { ID.Log, 5 }, { ID.Gravel, 15 } }, 100);
        AddStructureDefinition(ID.Anvil, new Dictionary<ID, int> { { ID.Steel, 8 }, { ID.Hammer, 1}}, 100);
        AddStructureDefinition(ID.BlueprintStation, new Dictionary<ID, int> { { ID.Stake, 10 }, { ID.Flint, 2 } }, 100);

        // Tools
        AddToolDefinition(
            stringID: ID.Sword,
            gesture: ItemGesture.Swing,
            speed: 1.3f,
            range: 1,
            projectileInfo: new SwingProjectileInfo {
                Damage = 2,
                Knockback = 15,
                CritChance = 10,
                Speed = 1.3f,
                Radius = 2f,
                Breaking = 1,
                OperationType = OperationType.Breaking
            },
            materials: new Dictionary<ID, int> { { ID.Flint, 5 }, { ID.Stake, 2 } },
            holdoutOffset: new Vector2(0.6f, 0)
        );

        AddToolDefinition(
            stringID: ID.Blueprint,
            gesture: ItemGesture.Swing,
            speed: 4f,
            range: 7f,
            materials: new Dictionary<ID, int> { { ID.Log, 2 } },
            holdoutOffset: new Vector2(0.65f, 0)
        );

        AddToolDefinition(
            stringID: ID.StonePickaxe,
            gesture: ItemGesture.Swing,
            speed: 1.4f,
            range: 4f,
            projectileInfo: new SwingProjectileInfo {
                Damage = 1,
                Knockback = 10,
                CritChance = 10,
                Speed = 2,
                Radius = 2,
                Breaking = 2,
                OperationType = OperationType.Mining
            },
            materials: new Dictionary<ID, int> { { ID.Sticks, 2 }, { ID.Flint, 4 } },
            holdoutOffset: new Vector2(0.65f, 0)
        );

        AddToolDefinition(
            stringID: ID.StoneHatchet,
            gesture: ItemGesture.Swing,
            speed: 1.4f,
            range: 4f,
            projectileInfo: new SwingProjectileInfo {
                Damage = 1,
                Knockback = 10,
                CritChance = 10,
                Speed = 2,
                Radius = 2,
                Breaking = 2,
                OperationType = OperationType.Breaking
            },
            materials: new Dictionary<ID, int> { { ID.Sticks, 2 }, { ID.Flint, 3 } },
            holdoutOffset: new Vector2(0.65f, 0)
        );

        AddToolDefinition(
            stringID: ID.MetalAxe,
            gesture: ItemGesture.Swing,
            speed: 1.4f,
            range: 4f,
            projectileInfo: new SwingProjectileInfo {
                Damage = 1,
                Knockback = 10,
                CritChance = 10,
                Speed = 2,
                Radius = 2,
                Breaking = 3,
                OperationType = OperationType.Breaking
            },
            materials: new Dictionary<ID, int> { { ID.Steel, 2 }, { ID.Stake, 5 } },
            holdoutOffset: new Vector2(0.65f, 0)
        );

        AddToolDefinition(
            stringID: ID.DiamondAxe,
            gesture: ItemGesture.Swing,
            speed: 1.4f,
            range: 4f,
            projectileInfo: new SwingProjectileInfo {
                Damage = 1,
                Knockback = 10,
                CritChance = 10,
                Speed = 2,
                Radius = 2,
                Breaking = 4,
                OperationType = OperationType.Breaking
            },
            materials: new Dictionary<ID, int> { { ID.Brick, 1 }, { ID.Stake, 2 } },
            holdoutOffset: new Vector2(0.65f, 0)
        );

        AddToolDefinition(
            stringID: ID.Hammer,
            gesture: ItemGesture.Swing,
            speed: 1.8f,
            range: 4f,
            projectileInfo: new SwingProjectileInfo {
                Damage = 1,
                Knockback = 10,
                CritChance = 10,
                Speed = 2,
                Radius = 2,
                Breaking = 3,
                OperationType = OperationType.Building
            },
            materials: new Dictionary<ID, int> { { ID.Flint, 2 }, { ID.Sticks, 2 } },
            holdoutOffset: new Vector2(0.65f, 0)
        );

        AddToolDefinition(
            stringID: ID.Spear,
            gesture: ItemGesture.Swing,
            speed: 0.8f,
            projectileInfo: new RangedProjectileInfo {
                Sprite = "spear",
                Damage = 2,
                Knockback = 10,
                CritChance = 10,
                LifeSpan = 10000,
                Speed = 60,
                Radius = 0.3f,
                Penetration = 1,
                Lodge = true,
                PickUp = true,
                Ammo = ID.Spear
            },
            materials: new Dictionary<ID, int> { { ID.Flint, 2 }, { ID.Stake, 2 } },
            projectileOffset: 1.016f,
            stackSize: 20,
            holdoutOffset: new Vector2(0.65f, 0),
            rotationOffset: 45
        );
        AddToolDefinition(
            stringID: ID.Minigun,
            gesture: ItemGesture.Shoot,
            speed: 2f,
            projectileInfo: new RangedProjectileInfo {
                Sprite = "bullet",
                Damage = 1,
                Knockback = 5,
                CritChance = 10,
                LifeSpan = 10000,
                Speed = 50,
                Radius = 0.1f,
                Penetration = 1,
                Scale = 0.6f
            },
            materials: new Dictionary<ID, int> { { ID.WoodBlock, 2 } },
            projectileOffset: 1.54f,
            holdoutOffset: new Vector2(0.4f, 0)
        );

        AddToolDefinition(
            stringID: ID.Pistol,
            gesture: ItemGesture.Shoot,
            speed: 0.6f,
            projectileInfo: new RangedProjectileInfo {
                Sprite = "bullet",
                Damage = 1,
                Knockback = 6,
                CritChance = 10,
                LifeSpan = 10000,
                Speed = 12,
                Radius = 0.1f,
                Ammo = ID.Bullet,
                Penetration = 1,
                Scale = 0.6f
            },
            materials: new Dictionary<ID, int> { { ID.WoodBlock, 2 } },
            projectileOffset: 1.016f,
            holdoutOffset: new Vector2(0.4f, 0)
        );
    }
    private static void AddMaterialDefinition(
        ID stringID,
        string description = "",
        Dictionary<ID, int> materials = null,
        int craftStack = 1,
        int time = 0,
        int stackSize = 15)
    {
        Entity.AddItem(stringID);

        Item itemData = new Item()
        {
            StringID = stringID,
            StackSize = stackSize,
            Rarity = ItemRarity.Common,
            Scale = 0.6f,

            Type = ItemType.Material,
            Gesture = ItemGesture.Swing,
            HoldoutOffset = new Vector2(0.5f, 0),

            Description = description
        };

        if (materials != null)
            ItemRecipe.AddRecipe(stringID, materials, craftStack, time, null);

        Dictionary[stringID] = itemData;
    }

    private static void AddBlockDefinition(
        ID stringID,
        int breakCost = 1,
        int breakThreshold = 1,
        SfxID sfx = SfxID.HitSand,
        string description = "",
        Dictionary<ID, int> materials = null,
        int craftStack = 1,
        int time = 0,
        int stackSize = 100)
    {
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

            Speed = 4,
            Range = 5,
            HoldoutOffset = new Vector2(0.5f, 0),

            Description = description
        };

        if (materials != null)
            ItemRecipe.AddRecipe(stringID, materials, craftStack, time,null);

        Dictionary[stringID] = itemData;
    }

    private static void AddToolDefinition(
        ID stringID,
        ItemGesture gesture,
        SfxID sfx = SfxID.Null,
        int stackSize = 1,
        ItemRarity rarity = ItemRarity.Common,

        float speed = 1,
        float range = 1,
        ProjectileInfo projectileInfo = null,
        int durability = 0,
        StatusEffect statusEffect = null,
        Vector2 holdoutOffset = new Vector2(),
        int rotationOffset = 0,
        float projectileOffset = 0,

        string description = "",
        Dictionary<ID, int> materials = null,
        int craftStack = 1,
        int time = 0,
        string[] modifiers = null
    )
    {
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
            Durability = durability,
            StatusEffect = statusEffect,
            ProjectileOffset = projectileOffset,
            HoldoutOffset = holdoutOffset,
            RotationOffset = rotationOffset,

            Description = description
        };

        if (materials != null)
            ItemRecipe.AddRecipe(stringID, materials, craftStack, time, modifiers);

        Dictionary[stringID] = itemData;
    }

    private static void AddStructureDefinition(
        ID stringID,
        Dictionary<ID, int> materials,
        int time = 200,
        SfxID sfx = SfxID.HitSand,
        string description = "")
    {
        Item itemData = new Item()
        {
            StringID = stringID,
            StackSize = 1,
            Rarity = ItemRarity.Common,

            Sfx = sfx,

            Type = ItemType.Structure,
            Gesture = ItemGesture.Swing,

            Speed = 1,
            Range = 5,

            Description = description
        };

        if (materials != null)
            StructureRecipe.AddRecipe(stringID, time, materials);

        Dictionary[stringID] = itemData;
    }

    public static Item GetItem(ID stringID)
    {
        if (Dictionary.ContainsKey(stringID))
        {
            return Dictionary[stringID];
        }
        return null;
    }

}
