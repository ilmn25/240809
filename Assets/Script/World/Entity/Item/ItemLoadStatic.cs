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
        AddBlockDefinition("sword", "Sword", 1, "Common", "A basic sword.", false, new string[] { "Iron", "Wood" }, 10, 2, 20);
    }    

    private static void AddBlockDefinition(string stringID, string name, int stackSize, string rarity, string description, Boolean consumable, string[] materials, int damage, int knockback, int useTime)
    {
        ItemData itemData = new ItemData(stringID, name, stackSize, rarity, description, consumable, materials, damage, knockback, useTime);
        _itemDefinitions[stringID] = itemData;
    }
    
    public void SpawnItem(string blockNameID, Vector3 worldPosition)
    {
        EntityData entityData = GetEntityData(blockNameID, worldPosition);
        
        GameObject gameObject = EntityPoolStatic.Instance.GetObject("item");
        gameObject.transform.position = entityData.Position.ToVector3(); 
        gameObject.GetComponent<SpriteRenderer>().sprite = 
            Resources.Load<Sprite>($"texture/sprite/{GetItem(entityData.ID).Name}"); 
        
        EntityHandler currentEntityHandler = gameObject.GetComponent<EntityHandler>();
        Vector3Int currentChunkCoordinate = WorldStatic.GetChunkCoordinate(gameObject.transform.position);
        EntityLoadStatic._entityList[currentChunkCoordinate].Item2.Add(currentEntityHandler); 
        currentEntityHandler.Initialize(entityData, currentChunkCoordinate);
         
    }

    public static EntityData GetEntityData(string stringID, Vector3 position)
    {
        return new EntityData(stringID, new SerializableVector3(position), type: EntityType.Item);
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