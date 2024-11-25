using System;
using System.Collections;
using System.Collections.Generic;
using Script.World.Entity.Item;
using UnityEngine;

public class ItemLoadStatic : MonoBehaviour
{
    public static ItemLoadStatic Instance { get; private set; }  
    private static Dictionary<string, ItemData> _itemDefinitions  = new Dictionary<string, ItemData>();
 
    public void Awake()
    {
        Instance = this;
        AddBlockDefinition("brick", "Brick", 20, ItemRarity.Common, "A basic brick.");
        AddBlockDefinition("marble", "Marble", 20, ItemRarity.Common, "A basic marble.", new Dictionary<string, int> {
            { "stone", 1 },
            { "brick", 1 }
        });
        AddBlockDefinition("dirt", "Dirt", 20, ItemRarity.Common, "A basic dirt.");
        AddBlockDefinition("backroom", "Backroom", 20, ItemRarity.Common, "A basic backroom.", new Dictionary<string, int> {
            { "dirt", 1 }
        });
        AddBlockDefinition("stone", "Stone", 20, ItemRarity.Common, "A basic stone.");
        AddToolDefinition("sword", "Sword", 1, ItemRarity.Common, "A basic sword.", false, 10, 2, 20, new Dictionary<string, int> {
            { "iron", 1 },
            { "wood", 2 }
        });
        AddToolDefinition("axe", "Axe", 1, ItemRarity.Common, "A basic sword.", false, 10, 2, 20, new Dictionary<string, int> {
            { "iron", 1 },
            { "wood", 2 }
        });
    }

    private static void AddBlockDefinition(string stringID, string name, int stackSize = 20, ItemRarity rarity = ItemRarity.Common,
        string description = "", Dictionary<string, int> materials = null)
    {
        ItemData itemData = new ItemData(stringID, name, stackSize, rarity, description, false, 0, 0, 0)
        {
            Type = ItemType.Block
        };
        if (materials != null) CraftStatic.AddCraftingDefinition(stringID, materials);
        _itemDefinitions[stringID] = itemData;
    }

    private static void AddToolDefinition(string stringID, string name, int stackSize = 1, ItemRarity rarity = ItemRarity.Common,
        string description = "", bool consumable = false, int damage = 0, float knockback = 0, int useTime = 0, Dictionary<string, int> materials = null)
    {
        ItemData itemData = new ItemData(stringID, name, stackSize, rarity, description, consumable, damage, knockback, useTime)
        {
            Type = ItemType.Tool
        };
        if (materials != null) CraftStatic.AddCraftingDefinition(stringID, materials);
        _itemDefinitions[stringID] = itemData;
    }

    private static void AddArmorDefinition(string stringID, string name, int stackSize = 1, ItemRarity rarity = ItemRarity.Common,
        string description = "", int defense = 0, Dictionary<string, int> materials = null)
    {
        ItemData itemData = new ItemData(stringID, name, stackSize, rarity, description, false, 0, 0, 0)
        {
            Type = ItemType.Armor,
            // Add additional armor-specific properties here if needed
        };
        if (materials != null) CraftStatic.AddCraftingDefinition(stringID, materials);
        _itemDefinitions[stringID] = itemData;
    }

    private static void AddAccessoryDefinition(string stringID, string name, int stackSize = 1, ItemRarity rarity = ItemRarity.Common,
        string description = "", bool consumable = false, Dictionary<string, int> materials = null)
    {
        ItemData itemData = new ItemData(stringID, name, stackSize, rarity, description, consumable, 0, 0, 0)
        {
            Type = ItemType.Accessory
        };
        if (materials != null) CraftStatic.AddCraftingDefinition(stringID, materials);
        _itemDefinitions[stringID] = itemData;
    }

    private static void AddFurnitureDefinition(string stringID, string name, int stackSize = 20, ItemRarity rarity = ItemRarity.Common,
        string description = "", Dictionary<string, int> materials = null)
    {
        ItemData itemData = new ItemData(stringID, name, stackSize, rarity, description, false, 0, 0, 0)
        {
            Type = ItemType.Furniture
        };
        if (materials != null) CraftStatic.AddCraftingDefinition(stringID, materials);
        _itemDefinitions[stringID] = itemData;
    }

    
    public void SpawnItem(string blockNameID, Vector3 worldPosition)
    {
        EntityData entityData = new EntityData(blockNameID, new SerializableVector3(worldPosition), type: EntityType.Item);
        
        GameObject gameObject = EntityPoolStatic.Instance.GetObject("item");
        gameObject.transform.position = entityData.Position.ToVector3(); 
        gameObject.GetComponent<SpriteRenderer>().sprite = 
            Resources.Load<Sprite>($"texture/sprite/{GetItem(entityData.ID).Name}"); 
        
        EntityHandler currentEntityHandler = gameObject.GetComponent<EntityHandler>();
        Vector3Int currentChunkCoordinate = WorldStatic.GetChunkCoordinate(gameObject.transform.position);
        EntityLoadStatic._entityList[currentChunkCoordinate].Item2.Add(currentEntityHandler); 
        currentEntityHandler.Initialize(entityData, currentChunkCoordinate);
         
    }

    public static ItemData GetItem(string stringID)
    {
        if (_itemDefinitions.ContainsKey(stringID))
        {
            return _itemDefinitions[stringID];
        }
        return null;
    }
  
}