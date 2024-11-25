using System;
using System.Collections;
using System.Collections.Generic;
using Script.World.Entity.Item;
using UnityEngine;

public class ItemLoadStatic : MonoBehaviour
{
    public static ItemLoadStatic Instance { get; private set; }  
    private static Dictionary<string, ItemData> _itemDefinitions  = new Dictionary<string, ItemData>();
    
    void Awake()
    {
        Instance = this;
        AddItemDefinition("brick", "Brick", 20, ItemRarity.Common, "A basic brick.");
        AddItemDefinition("marble", "Marble", 20, ItemRarity.Common, "A basic marble.");
        AddItemDefinition("dirt", "Dirt", 20, ItemRarity.Common, "A basic dirt.");
        AddItemDefinition("backroom", "Backroom", 20, ItemRarity.Common, "A basic backroom.");
        AddItemDefinition("stone", "Stone", 20, ItemRarity.Common, "A basic stone.");
        AddItemDefinition("sword", "Sword", 1, ItemRarity.Common, "A basic sword.", false, 10, 2, 20,  
            new Dictionary<string, int> {
                {"iron", 1}, 
                {"wood", 2}
            });
    }    

    private static void AddItemDefinition(string stringID, string name, int stackSize = 20, ItemRarity rarity = ItemRarity.Common,
        string description = "", Boolean consumable = true, int damage = 0, int knockback = 0, int useTime = 0, Dictionary<string, int> materials = null)
    {
        ItemData itemData = new ItemData(stringID, name, stackSize, rarity, description, consumable, damage, knockback, useTime);
        if (materials != null) CraftStatic.AddCraftingDefinition(stringID, materials);
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