using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Item 
{
    private static readonly Dictionary<ID, Item> Dictionary = new ();
    public ID StringID; 
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
        string rawName = StringID.ToString();
        return System.Text.RegularExpressions.Regex.Replace(rawName, "(?<!^)([A-Z])", " $1").ToLower();
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

            Speed = Game.BuildMode? 70 : 4,
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
