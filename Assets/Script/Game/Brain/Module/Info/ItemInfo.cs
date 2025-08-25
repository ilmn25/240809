using System;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class ItemInfo : Info
{
    public ItemSlot item;
    public int despawn;
    [NonSerialized] public bool StackOnSpawn = true;
    [NonSerialized] public Vector3 Velocity;
    [NonSerialized] public SpriteRenderer SpriteRenderer; 

    public override void Initialize()
    {
        if (item == null) item = new ItemSlot
        {
            ID = stringID,
            Stack = 1,
        };
        SpriteRenderer = Machine.transform.GetComponent<SpriteRenderer>();
    }

    public override void Update()
    {
        if (despawn > 0)
        {
            despawn--;
            if (despawn == 0) Destroy();
        }
        
        if (Machine)
        {
            position = Machine.transform.position;
            IsInRenderRange = SpriteRenderer.isVisible &&
                              MapLoad.ActiveChunks.ContainsKey(World.GetChunkCoordinate(Machine.transform.position));
        }
    }

    public void OnActionSecondary(Info info)
    {        
        if (Vector3.Distance(position, info.Machine.transform.position) < 3f) 
        { 
            Audio.PlaySFX(SfxID.Item);
            ((PlayerInfo)info).Storage.AddItem(item);
            Inventory.RefreshInventory();
            Destroy();
        }
    }
}