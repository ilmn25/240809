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

    // Shared construction for every item type (sprite scale, hand offset,
    // rarity, description). Type-specific fields are set by each Add* method.
    private static Item Make(ID id, ItemType type, string description, int stackSize, ItemRarity rarity = ItemRarity.Common, float scale = 0.6f)
    {
        return new Item
        {
            ID = id,
            Type = type,
            Gesture = ItemGesture.Swing,
            StackSize = stackSize,
            Rarity = rarity,
            Scale = scale,
            HoldoutOffset = new Vector2(0.5f, 0),
            Description = description,
        };
    }

    // Registers the item and its optional craft recipe in one place.
    private static void Register(Item itemData, Dictionary<ID, int> materials = null, int craftStack = 1, int time = 0, string[] modifiers = null)
    {
        if (materials != null)
            ItemRecipe.AddRecipe(itemData.ID, materials, craftStack, time, modifiers);
        Dictionary[itemData.ID] = itemData;
    }

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
        Item itemData = Make(id, ItemType.Material, description, stackSize);
        itemData.Pickupable = pickupable;
        itemData.DespawnTime = despawnTime;
        itemData.BurnResult = burnResult;
        Register(itemData, materials, craftStack, time);
    }

    private static void AddRelicDefinition(ID id, string description, ItemRarity rarity = ItemRarity.Rare)
    {
        Item itemData = Make(id, ItemType.Material, description, 1, rarity);
        Register(itemData);
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
        Item itemData = Make(id, ItemType.Consumable, description, stackSize);
        itemData.HungerValue = hungerValue;
        itemData.HealValue = healValue;
        itemData.DamageValue = damageValue;
        itemData.MaxHpBonus = maxHpBonus;
        itemData.MaxHungerBonus = maxHungerBonus;
        Register(itemData, materials, craftStack, time);
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

        Item itemData = Make(id, ItemType.Block, description, stackSize);
        itemData.Sfx = sfx;
        itemData.Speed = 4;
        itemData.Range = 5;
        itemData.BurnResult = burnResult;
        Register(itemData, materials, craftStack, time);
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

        Item itemData = Make(id, ItemType.Tool, description, stackSize, rarity, scale: 1);
        itemData.Gesture = gesture;
        itemData.Sfx = sfx;
        itemData.Speed = speed;
        itemData.Range = range;
        itemData.ProjectileInfo = projectileInfo;
        itemData.Durability = durability;
        itemData.StatusEffect = statusEffect;
        itemData.ProjectileOffset = projectileOffset;
        itemData.HoldoutOffset = holdoutOffset;
        itemData.RotationOffset = rotationOffset;
        itemData.Glow = glow;
        Register(itemData, materials, craftStack, time, modifiers);
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
        Item itemData = Make(id, ItemType.Structure, description, 1, scale: 1);
        itemData.Sfx = sfx;
        itemData.Furniture = furniture;
        itemData.Speed = 1;
        itemData.Range = 5;
        itemData.BurnResult = burnResult;
        Register(itemData, materials, 1, time);
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
