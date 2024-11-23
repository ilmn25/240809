using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemLoadStatic : MonoBehaviour
{
    public static ItemLoadStatic Instance { get; private set; }  
    private static Dictionary<int, ItemData> _itemDefinitions  = new Dictionary<int, ItemData>();
    private static IntStringMap<int, string> _itemIDMap = new IntStringMap<int, string>();
    private static int _nextItemID = 1;
    
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
        int id = _nextItemID++;
        ItemData itemData = new ItemData(stringID, name, stackSize, rarity, description, consumable, materials, damage, knockback, useTime);
        _itemDefinitions[id] = itemData;
        _itemIDMap.Add(id, stringID);
    }
    
    public void SpawnItem(EntityData entityData)
    { 
        ItemData itemDataToSpawn = GetItem(int.Parse(entityData.ID));
        GameObject itemObject = Instantiate(Resources.Load<GameObject>($"prefab/item"));
        itemObject.name = entityData.ID;
        itemObject.transform.parent = transform;
        itemObject.transform.position = entityData.Position.ToVector3(); 
        
        SpriteRenderer spriteRenderer = itemObject.GetComponent<SpriteRenderer>(); 
        spriteRenderer.sprite = Resources.Load<Sprite>($"texture/sprite/{itemDataToSpawn.Name}");
        EntityHandler entityHandler = itemObject.GetComponent<EntityHandler>();
        entityHandler._entityData = entityData;
    }

    public static EntityData GetEntityData(int id, Vector3 position, EntityType entityType)
    {
        return new EntityData(id.ToString(), new SerializableVector3(position), type: entityType);
    } 
    

    public static ItemData GetItem(int id)
    {
        if (_itemDefinitions.ContainsKey(id))
        {
            return _itemDefinitions[id];
        }
        return null;
    }

    public static ItemData GetItem(string stringID)
    {
        int id = _itemIDMap.StringtoInt[stringID];
        if (_itemDefinitions.ContainsKey(id))
        {
            return _itemDefinitions[id];
        }
        return null;
    }
 
    public static int ConvertID(string stringID)
    {
        return _itemIDMap.StringtoInt[stringID];
        //return -1?
    }

    public static string ConvertID(int id)
    {
        if (id == 0) return null;
        return _itemIDMap.InttoString[id];
    }
}