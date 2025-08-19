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
                    GUIMain.Storage.Storage.AddItem(Storage.List[CurrentSlotKey].StringID, 
                        Storage.List[CurrentSlotKey].Stack);
                    Storage.RemoveItem(Storage.List[CurrentSlotKey].StringID, 
                        Storage.List[CurrentSlotKey].Stack, CurrentSlotKey); 
                }
                else
                {
                    Entity.SpawnItem(Storage.List[CurrentSlotKey].StringID, Game.PlayerInfo.position, 
                        Storage.List[CurrentSlotKey].Stack);
                    Storage.RemoveItem(Storage.List[CurrentSlotKey].StringID, 
                        Storage.List[CurrentSlotKey].Stack, CurrentSlotKey); 
                }
                //doesnt account for full inventory
            }
            else
            {
                GUIMain.StorageInv.Storage.AddItem(Storage.List[CurrentSlotKey].StringID, 
                    Storage.List[CurrentSlotKey].Stack);
                Storage.RemoveItem(Storage.List[CurrentSlotKey].StringID, 
                    Storage.List[CurrentSlotKey].Stack, CurrentSlotKey); 
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
                (Storage.List[CurrentSlotKey], GUICursor.Data) = 
                    (GUICursor.Data, Storage.List[CurrentSlotKey]);
            } 
        } 
        Audio.PlaySFX("pick_up", 0.4f);
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
                Audio.PlaySFX("pick_up", 0.4f);
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
                Audio.PlaySFX("pick_up", 0.4f);
                Inventory.RefreshInventory();
            }
        }
    }
    protected override void SetInfoPanel(ItemSlot itemSlot)
    {
        GUIMain.Cursor.Set(itemSlot.StringID);
        GUIMain.InfoPanel.Set(itemSlot.GetItemInfo(false));
    }
}