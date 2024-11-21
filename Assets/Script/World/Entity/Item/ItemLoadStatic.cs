using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemLoadStatic : MonoBehaviour
{
    public static ItemLoadStatic Instance { get; private set; }  
    public static List<ItemData> itemList = new List<ItemData>();

    void Awake()
    {
        Instance = this;
        itemList.Add(new ItemData(1, "brick", 1, "Common", 10, 2.5f, 20, "A basic sword.", false, new string[] { "Iron", "Wood" }));
        itemList.Add(new ItemData(2, "marble", 1, "Common", 10, 2.5f, 20, "A basic sword.", false, new string[] { "Iron", "Wood" }));
        itemList.Add(new ItemData(3, "dirt", 1, "Common", 10, 2.5f, 20, "A basic sword.", false, new string[] { "Iron", "Wood" }));
        itemList.Add(new ItemData(4, "backroom", 1, "Common", 10, 2.5f, 20, "A basic sword.", false, new string[] { "Iron", "Wood" }));
        itemList.Add(new ItemData(5, "stone", 1, "Common", 10, 2.5f, 20, "A basic sword.", false, new string[] { "Iron", "Wood" }));
        itemList.Add(new ItemData(6, "Sword", 1, "Common", 10, 2.5f, 20, "A basic sword.", false, new string[] { "Iron", "Wood" }));
    }

    public void SpawnItem(EntityData entityData)
    { 
        ItemData itemDataToSpawn = itemList.Find(item => item.ID == int.Parse(entityData.ID));
        GameObject itemObject = Instantiate(Resources.Load<GameObject>($"prefab/item"));
        itemObject.name = entityData.ID;
        itemObject.transform.parent = transform; 
        itemObject.transform.position = entityData.Position.ToVector3(); 
        
        SpriteRenderer spriteRenderer = itemObject.GetComponent<SpriteRenderer>(); 
        spriteRenderer.sprite = Resources.Load<Sprite>($"texture/sprite/{itemDataToSpawn.Name}");
        EntityAbstract entityAbstract = itemObject.GetComponent<EntityAbstract>();
        entityAbstract._entityData = entityData;
    }

    public static EntityData GetEntityData(int id, Vector3 position, EntityType entityType)
    {
        return new EntityData(id.ToString(), new SerializableVector3(position), type: entityType);
    }
    
    public static int GetItemNameID(string itemName)
    {
        ItemData itemData = itemList.Find(i => i.Name == itemName);
        return itemData != null ? itemData.ID : -1; // Return -1 if the item is not found
    }
}