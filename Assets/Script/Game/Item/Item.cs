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
    
    public float Scale; 
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
     
    public string Name => GetName(); 
    public string Description;

    private string GetName()
    {
        string rawName = ID.ToString();
        return System.Text.RegularExpressions.Regex.Replace(rawName, "(?<!^)([A-Z])", " $1").ToLower();
    }

    private static void AddMaterialDefinition(
        ID id,
        string description = "",
        Dictionary<ID, int> materials = null,
        int craftStack = 1,
        int time = 0,
        int stackSize = 15)
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
        int stackSize = 100)
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
        string[] modifiers = null
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
        string description = "")
    {
        Item itemData = new Item()
        {
            ID = id,
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
            StructureRecipe.AddRecipe(id, time, materials);

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
