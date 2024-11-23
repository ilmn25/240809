using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemLoadStatic : MonoBehaviour
{
    public static ItemLoadStatic Instance { get; private set; }  
    private static Dictionary<string, ItemData> _itemDefinitions  = new Dictionary<string, ItemData>();
    
    // id of item in entitydata is number id
    // id of item in itemdata is name id 
    void Awake()
    {
        Instance = this;
        AddBlockDefinition("brick", "Brick", 1, "Common", "A basic brick.", false, new string[] { "Iron", "Wood" }, 10, 2, 20);
        AddBlockDefinition("marble", "Marble", 1, "Common", "A basic marble.", false, new string[] { "Iron", "Wood" }, 10, 2, 20);
        AddBlockDefinition("dirt", "Dirt", 1, "Common", "A basic dirt.", false, new string[] { "Iron", "Wood" }, 10, 2, 20);
        AddBlockDefinition("backroom", "Backroom", 1, "Common", "A basic backroom.", false, new string[] { "Iron", "Wood" }, 10, 2, 20);
        AddBlockDefinition("stone", "Stone", 1, "Common", "A basic stone.", false, new string[] { "Iron", "Wood" }, 10, 2, 20);
        AddBlockDefinition("Sword", "Sword", 1, "Common", "A basic sword.", false, new string[] { "Iron", "Wood" }, 10, 2, 20);
    }


    private static void AddBlockDefinition(string stringID, string name, int stackSize, string rarity, string description, Boolean consumable, string[] materials, int damage, int knockback, int useTime)
    {
        ItemData itemData = new ItemData(stringID, name, stackSize, rarity, description, consumable, materials, damage, knockback, useTime);
        _itemDefinitions[stringID] = itemData;
    }
    
    public void SpawnItem(EntityData entityData)
    { 
        ItemData itemDataToSpawn = GetItem(entityData.ID);
        GameObject itemObject = EntityPoolStatic.Instance.GetObject(entityData, "item");
        itemObject.transform.position = entityData.Position.ToVector3(); 
        
        SpriteRenderer spriteRenderer = itemObject.GetComponent<SpriteRenderer>(); 
        spriteRenderer.sprite = Resources.Load<Sprite>($"texture/sprite/{itemDataToSpawn.Name}"); 
    }

    public static EntityData GetEntityData(string stringID, Vector3 position, EntityType entityType)
    {
        return new EntityData(stringID, new SerializableVector3(position), type: entityType);
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