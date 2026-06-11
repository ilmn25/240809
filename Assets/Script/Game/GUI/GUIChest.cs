using Mirror;
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
                    // Suppress the auto-sync hook during bulk transfer
                    Storage.SuppressSync = true;
                    GUIMain.Storage.Storage.SuppressSync = true;
                    GUIMain.Storage.Storage.AddItem(Storage.List[CurrentSlotKey]);
                    Storage.SuppressSync = false;
                    GUIMain.Storage.Storage.SuppressSync = false;
                    // Atomic two‑way transfer via StorageSync
                    StorageSync.SendTransfer(
                        Storage.info.uid, Storage,
                        GUIMain.Storage.Storage.info.uid, GUIMain.Storage.Storage);
                }
                else if (Helper.IsHost())
                {
                    Entity.SpawnItem(Storage.List[CurrentSlotKey], Main.PlayerInfo.position); 
                }
                else if (NetworkClient.isConnected)
                {
                    int dropAmount = Storage.List[CurrentSlotKey].Stack;
                    Inventory.ClientDropSlot(Storage.List[CurrentSlotKey], dropAmount, Storage, Main.PlayerInfo.position);
                }
                //doesnt account for full inventory
            }
            else
            {
                // Suppress the auto-sync hook during bulk transfer
                Storage.SuppressSync = true;
                GUIMain.StorageInv.Storage.SuppressSync = true;
                GUIMain.StorageInv.Storage.AddItem(Storage.List[CurrentSlotKey]);
                Storage.SuppressSync = false;
                GUIMain.StorageInv.Storage.SuppressSync = false;
                // Atomic two‑way transfer via StorageSync
                StorageSync.SendTransfer(
                    Storage.info.uid, Storage,
                    GUIMain.StorageInv.Storage.info.uid, GUIMain.StorageInv.Storage);
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
        Storage.NotifyChanged();
        // Single‑storage sync is handled by Storage.OnChanged hook
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
                Storage.NotifyChanged();
                // Sync is handled by Storage.OnChanged hook
            }
        }
    }
    protected override void SetInfoPanel(ItemSlot itemSlot)
    {
        GUIMain.Cursor.SetItemSlotInfo(itemSlot, false);
        // GUIMain.InfoPanel.Set(itemSlot.GetItemInfo(false));
    }
}