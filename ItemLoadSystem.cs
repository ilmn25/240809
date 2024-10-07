using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemLoadSystem : MonoBehaviour
{
    public static List<Item> itemList = new List<Item>();

    void Awake()
    {
        itemList.Add(new Item(1, "brick", 1, "Common", 10, 2.5f, 20, "A basic sword.", false, new string[] { "Iron", "Wood" }));
        itemList.Add(new Item(2, "marble", 1, "Common", 10, 2.5f, 20, "A basic sword.", false, new string[] { "Iron", "Wood" }));
        itemList.Add(new Item(3, "dirt", 1, "Common", 10, 2.5f, 20, "A basic sword.", false, new string[] { "Iron", "Wood" }));
        itemList.Add(new Item(4, "backroom", 1, "Common", 10, 2.5f, 20, "A basic sword.", false, new string[] { "Iron", "Wood" }));
        itemList.Add(new Item(5, "stone", 1, "Common", 10, 2.5f, 20, "A basic sword.", false, new string[] { "Iron", "Wood" }));
        itemList.Add(new Item(6, "Sword", 1, "Common", 10, 2.5f, 20, "A basic sword.", false, new string[] { "Iron", "Wood" }));
    }

    // private void Start() 
    // {  
    //     SpawnItem(GetItemNameID("brick"), new Vector3(0,5,0)); 
    //     SpawnItem(GetItemNameID("brick"), new Vector3(0,5,1));   
    //     SpawnItem(GetItemNameID("brick"), new Vector3(0,5,3));     
    //     SpawnItem(GetItemNameID("brick"), new Vector3(3,5,3));     
    // }

    public EntityHandler SpawnItem(int id, Vector3 position)
    {
        Item itemToSpawn = itemList.Find(item => item.ID == id);
        GameObject itemObject = new GameObject(itemToSpawn.ID.ToString());
        itemObject.transform.parent = transform; 

        itemObject.transform.position = position;
        // itemObject.transform.rotation = Quaternion.Euler(90, 0, 0);
        Vector3 scale = itemObject.transform.localScale;
        scale.y = Mathf.Sqrt(2);
        itemObject.transform.localScale = scale / 2; 

        SpriteRenderer spriteRenderer = itemObject.AddComponent<SpriteRenderer>();
        Material material = Resources.Load<Material>("shader/material/sprite_shadow");
        spriteRenderer.material = material;
        Sprite sprite = Resources.Load<Sprite>($"texture/sprite/{itemToSpawn.Name}");
        spriteRenderer.sprite = sprite;
        // spriteRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        spriteRenderer.receiveShadows = true;
         
        itemObject.AddComponent<ItemPhysicsHandler>().PopItem(); 
        itemObject.AddComponent<SpriteOrbitHandler>();
        itemObject.AddComponent<SpriteYCullHandler>();   
        itemObject.AddComponent<EntityChunkPositionHandler>();
        EntityHandler entityHandler = itemObject.AddComponent<EntityHandler>();
        entityHandler._entity = new Entity(itemToSpawn.ID.ToString(), new SerializableVector3(position), type: EntityType.Item);
        return entityHandler;
    }

    public int GetItemNameID(string itemName)
    {
        Item item = itemList.Find(i => i.Name == itemName);
        return item != null ? item.ID : -1; // Return -1 if the item is not found
    }
}

[System.Serializable]
public class Item
{
    public int ID { get; set; }
    public string Name { get; set; }
    public int StackSize { get; set; }
    public string Rarity { get; set; }
    public int Damage { get; set; }
    public float Knockback { get; set; }
    public int UseTime { get; set; }
    public string Description { get; set; }
    public bool IsConsumable { get; set; }
    public string[] CraftingMaterials { get; set; }

    public Item(int id, string name, int stackSize, string rarity, int damage, float knockback, int useTime, string description, bool isConsumable, string[] craftingMaterials)
    {
        ID = id;
        Name = name;
        StackSize = stackSize;
        Rarity = rarity;
        Damage = damage;
        Knockback = knockback;
        UseTime = useTime;
        Description = description;
        IsConsumable = isConsumable;
        CraftingMaterials = craftingMaterials;
    }
}