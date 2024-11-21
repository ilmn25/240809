using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemLoadStatic : MonoBehaviour
{
    public static List<ItemData> itemList = new List<ItemData>();

    void Awake()
    {
        itemList.Add(new ItemData(1, "brick", 1, "Common", 10, 2.5f, 20, "A basic sword.", false, new string[] { "Iron", "Wood" }));
        itemList.Add(new ItemData(2, "marble", 1, "Common", 10, 2.5f, 20, "A basic sword.", false, new string[] { "Iron", "Wood" }));
        itemList.Add(new ItemData(3, "dirt", 1, "Common", 10, 2.5f, 20, "A basic sword.", false, new string[] { "Iron", "Wood" }));
        itemList.Add(new ItemData(4, "backroom", 1, "Common", 10, 2.5f, 20, "A basic sword.", false, new string[] { "Iron", "Wood" }));
        itemList.Add(new ItemData(5, "stone", 1, "Common", 10, 2.5f, 20, "A basic sword.", false, new string[] { "Iron", "Wood" }));
        itemList.Add(new ItemData(6, "Sword", 1, "Common", 10, 2.5f, 20, "A basic sword.", false, new string[] { "Iron", "Wood" }));
    }

    // private void Start() 
    // {  
    //     SpawnItem(GetItemNameID("brick"), new Vector3(0,5,0)); 
    //     SpawnItem(GetItemNameID("brick"), new Vector3(0,5,1));   
    //     SpawnItem(GetItemNameID("brick"), new Vector3(0,5,3));     
    //     SpawnItem(GetItemNameID("brick"), new Vector3(3,5,3));     
    // }

    public EntityAbstract SpawnItem(int id, Vector3 position)
    {
        ItemData itemDataToSpawn = itemList.Find(item => item.ID == id);
        GameObject itemObject = new GameObject(itemDataToSpawn.ID.ToString());
        itemObject.transform.parent = transform; 

        itemObject.transform.position = position;
        // itemObject.transform.rotation = Quaternion.Euler(90, 0, 0);
        Vector3 scale = itemObject.transform.localScale;
        scale.y = Mathf.Sqrt(2);
        itemObject.transform.localScale = scale / 2; 

        SpriteRenderer spriteRenderer = itemObject.AddComponent<SpriteRenderer>();
        Material material = Resources.Load<Material>("shader/material/sprite_shadow");
        spriteRenderer.material = material;
        Sprite sprite = Resources.Load<Sprite>($"texture/sprite/{itemDataToSpawn.Name}");
        spriteRenderer.sprite = sprite;
        // spriteRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        spriteRenderer.receiveShadows = true;
         
        itemObject.AddComponent<ItemPhysicInst>().PopItem(); 
        itemObject.AddComponent<SpriteOrbitInst>();
        itemObject.AddComponent<SpriteCullInst>();   
        itemObject.AddComponent<EntityChunkPositionInst>();
        EntityAbstract entityAbstract = itemObject.AddComponent<EntityAbstract>();
        entityAbstract.entityData = new EntityData(itemDataToSpawn.ID.ToString(), new SerializableVector3(position), type: EntityType.Item);
        return entityAbstract;
    }

    public int GetItemNameID(string itemName)
    {
        ItemData itemData = itemList.Find(i => i.Name == itemName);
        return itemData != null ? itemData.ID : -1; // Return -1 if the item is not found
    }
}