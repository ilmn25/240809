using UnityEngine;

[System.Serializable]
public class ItemInfo : Info
{
    public ItemSlot item;

    public override void Initialize()
    {
        if (item == null) item = new ItemSlot
        {
            ID = stringID,
            Stack = 1,
        };
    }

    public override void Update()
    {
        if (Machine) position = Machine.transform.position;
    }

    public void OnActionSecondary(Info info)
    {        
        if (Vector3.Distance(position, info.Machine.transform.position) < 3f) 
        { 
            Audio.PlaySFX(SfxID.Item);
            ((PlayerInfo)info).Storage.AddItem(item.ID, item.Stack);
            Inventory.RefreshInventory();
            Destroy();
        }
    }
}