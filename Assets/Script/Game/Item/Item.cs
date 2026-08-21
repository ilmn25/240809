using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Item 
{
    private static readonly Dictionary<ID, Item> Dictionary = new ();
    public ID ID; 
    public int StackSize;
    public ItemRarity Rarity; 
    
    public float Scale = 1; 
    public SfxID Sfx; 
    
    public ItemType Type; 
    public ItemGesture Gesture;
    
    public float Speed;
    public float Range;
    public ProjectileInfo ProjectileInfo;
    public int Durability = -1;
    public StatusEffect StatusEffect; 
    public float ProjectileOffset;
    public Vector2 HoldoutOffset;
    public float RotationOffset = 90;
    public int HungerValue; // hunger restored when eaten (0 = not food)
    public int HealValue; // health restored when consumed (0 = no direct heal)
    public int DamageValue; // damage dealt to the eater when consumed (0 = safe)
    public int MaxHpBonus; // permanent max-health increase when consumed (0 = none)
    public int MaxHungerBonus; // permanent max-hunger increase when consumed (0 = none)
    public bool Glow; // lights up the held tool's Glow light (torch, ...)

    /// <summary>What a burnable item turns into when burned. Null = not burnable
    /// (single source of truth: no separate Burnable flag).</summary>
    public ID BurnResult = ID.Null;

    /// <summary>Whether this structure places directly as furniture: no build phase,
    /// can't be broken, and a hammer picks it back up into the player's inventory.</summary>
    public bool Furniture;

    /// <summary>Whether this item can be picked up by the player. False for
    /// cosmetic debris (blood) that should just sit on the ground.</summary>
    public bool Pickupable = true;
    /// <summary>If &gt; 0, this item despawns after this many seconds on the ground.</summary>
    public float DespawnTime;
     
    public string Name => Helper.ToDisplayName(ID, lowercase: true);
    public string Description;

    /// <summary>Short action shown in the HUD for the held item (e.g. "place", "eat").
    /// Empty when the item has no quick action.</summary>
    public string ActionLabel => Type switch
    {
        ItemType.Block or ItemType.Structure => "place",
        ItemType.Consumable => "consume",
        ItemType.Tool => "use",
        _ => "",
    };

    private static void AddMaterialDefinition(
        ID id,
        string description = "",
        Dictionary<ID, int> materials = null,
        int craftStack = 1,
        int time = 0,
        int stackSize = 15,
        bool pickupable = true,
        float despawnTime = 0f,
        ID burnResult = ID.Null)
    {

        Item itemData = new Item()
        {
            ID = id,
            StackSize = stackSize,
            Rarity = ItemRarity.Common,
            Scale = 0.6f,

            Type = ItemType.Material,
            Gesture = ItemGesture.Swing,
            HoldoutOffset = new Vector2(0.5f, 0),

            Pickupable = pickupable,
            DespawnTime = despawnTime,

            BurnResult = burnResult,

            Description = description
        };

        if (materials != null)
            ItemRecipe.AddRecipe(id, materials, craftStack, time, null);

        Dictionary[id] = itemData;
    }

    private static void AddRelicDefinition(ID id, string description, ItemRarity rarity = ItemRarity.Rare)
    {
        Item itemData = new Item()
        {
            ID = id,
            StackSize = 1,
            Rarity = rarity,
            Scale = 0.6f,

            Type = ItemType.Material,
            Gesture = ItemGesture.Swing,
            HoldoutOffset = new Vector2(0.5f, 0),

            Description = description
        };

        Dictionary[id] = itemData;
    }

    private static void AddConsumableDefinition(
        ID id,
        int hungerValue,
        string description = "",
        Dictionary<ID, int> materials = null,
        int craftStack = 1,
        int time = 0,
        int stackSize = 15,
        int healValue = 0,
        int damageValue = 0,
        int maxHpBonus = 0,
        int maxHungerBonus = 0)
    {
        Item itemData = new Item()
        {
            ID = id,
            StackSize = stackSize,
            Rarity = ItemRarity.Common,
            Scale = 0.6f,

            Type = ItemType.Consumable,
            Gesture = ItemGesture.Swing,
            HoldoutOffset = new Vector2(0.5f, 0),
            HungerValue = hungerValue,
            HealValue = healValue,
            DamageValue = damageValue,
            MaxHpBonus = maxHpBonus,
            MaxHungerBonus = maxHungerBonus,

            Description = description
        };

        if (materials != null)
            ItemRecipe.AddRecipe(id, materials, craftStack, time, null);

        Dictionary[id] = itemData;
    }

    private static void AddBlockDefinition(
        ID id,
        int breakCost = 1,
        int breakThreshold = 1,
        SfxID sfx = SfxID.HitSand,
        string description = "",
        Dictionary<ID, int> materials = null,
        int craftStack = 1,
        int time = 0,
        int stackSize = 100,
        ID burnResult = ID.Null)
    {
        Entity.AddBlock(id);
        Block.AddBlockDefinition(id, breakThreshold, breakCost);

        Item itemData = new Item()
        {
            ID = id,
            StackSize = stackSize,
            Rarity = ItemRarity.Common,

            Scale = 0.6f,
            Sfx = sfx,

            Type = ItemType.Block,
            Gesture = ItemGesture.Swing,

            Speed = 4,
            Range = 5,
            HoldoutOffset = new Vector2(0.5f, 0),

            BurnResult = burnResult,

            Description = description
        };

        if (materials != null)
            ItemRecipe.AddRecipe(id, materials, craftStack, time,null);

        Dictionary[id] = itemData;
    }

    private static void AddToolDefinition(
        ID id,
        ItemGesture gesture,
        SfxID sfx = SfxID.Sword,
        int stackSize = 1,
        ItemRarity rarity = ItemRarity.Common,

        float speed = 1,
        float range = 1,
        ProjectileInfo projectileInfo = null,
        int durability = 200,
        StatusEffect statusEffect = null,
        Vector2 holdoutOffset = new Vector2(),
        int rotationOffset = 0,
        float projectileOffset = 0,

        string description = "",
        Dictionary<ID, int> materials = null,
        int craftStack = 1,
        int time = 0,
        string[] modifiers = null,
        bool glow = false
    )
    {

        Item itemData = new Item()
        {
            ID = id,
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
            Glow = glow,

            Description = description
        };

        if (materials != null)
            ItemRecipe.AddRecipe(id, materials, craftStack, time, modifiers);

        Dictionary[id] = itemData;
    }

    private static void AddStructureDefinition(
        ID id,
        Dictionary<ID, int> materials,
        int time = 200,
        SfxID sfx = SfxID.HitSand,
        string description = "",
        bool furniture = false,
        ID burnResult = ID.Null)
    {
        Item itemData = new Item()
        {
            ID = id,
            StackSize = 1,
            Rarity = ItemRarity.Common,

            Sfx = sfx,

            Type = ItemType.Structure,
            Furniture = furniture,
            Gesture = ItemGesture.Swing,

            Speed = 1,
            Range = 5,
            HoldoutOffset = new Vector2(0.5f, 0),

            BurnResult = burnResult,

            Description = description
        };

        if (materials != null)
        {
            ItemRecipe.AddRecipe(id, materials, 1, time, null);
        }

        Dictionary[id] = itemData;
    }

    public static Item GetItem(ID id)
    {
        if (Dictionary.ContainsKey(id))
        {
            return Dictionary[id];
        }
        return null;
    }

}
