using UnityEngine;

public class GUIChest : GUIStorage
{
    protected override void ActionPrimaryDown()
    {
        if (Storage.List[CurrentSlotKey].isEmpty() && GUICursor.Data.isEmpty())
        {
            return;
        }
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (GUIMain.StorageInv == this)
            {
                if (GUIMain.Storage.Showing)
                {
                    GUIMain.Storage.Storage.AddItem(Storage.List[CurrentSlotKey]);
                }
                else
                {
                    Entity.SpawnItem(Storage.List[CurrentSlotKey], Game.PlayerInfo.position); 
                }
                //doesnt account for full inventory
            }
            else
            {
                GUIMain.StorageInv.Storage.AddItem(Storage.List[CurrentSlotKey]);
                //doesnt account for full inventory
            } 
        }
        else
        { 
            if (GUICursor.Data.isEmpty())
            { 
                GUICursor.Data.Add(Storage.List[CurrentSlotKey]);
            } 
            else if (Storage.List[CurrentSlotKey].isSame(GUICursor.Data))
            { 
                Storage.List[CurrentSlotKey].Add(GUICursor.Data);
            } 
            else
            {
                ItemSlot item = new ItemSlot();
                item.Add(GUICursor.Data);
                GUICursor.Data.Add(Storage.List[CurrentSlotKey]);
                Storage.List[CurrentSlotKey].Add(item);
            } 
        } 
        Audio.PlaySFX(SfxID.Item);
        Inventory.RefreshInventory();
    }

    protected override void ActionSecondaryDown()
    {
        if (!Input.GetKey(KeyCode.LeftShift)) return;
        ItemSlot itemSlot = Storage.List[CurrentSlotKey];
        if (!itemSlot.isEmpty())
        {
            if (GUICursor.Data.isEmpty() || itemSlot.isSame(GUICursor.Data))
            {
                GUICursor.Data.Add(itemSlot, itemSlot.Stack/2); 
                Audio.PlaySFX(SfxID.Item);
                Inventory.RefreshInventory();
            }
        }
    }

    protected override void ActionSecondaryKey()
    {
        ItemSlot itemSlot = Storage.List[CurrentSlotKey];
        if (!itemSlot.isEmpty())
        {
            if (GUICursor.Data.isEmpty() || itemSlot.isSame(GUICursor.Data))
            {
                GUICursor.Data.Add(itemSlot, 1);
                Audio.PlaySFX(SfxID.Item);
                Inventory.RefreshInventory();
            }
        }
    }
    protected override void SetInfoPanel(ItemSlot itemSlot)
    {
        GUIMain.Cursor.Set(itemSlot, false);
        // GUIMain.InfoPanel.Set(itemSlot.GetItemInfo(false));
    }
}