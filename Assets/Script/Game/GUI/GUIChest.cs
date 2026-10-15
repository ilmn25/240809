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
            if (TrySell(Storage.List[CurrentSlotKey]))
            {
                Audio.PlaySFX(SfxID.Item);
                return;
            }

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
                else
                {
                    Inventory.DropToWorld(Storage.List[CurrentSlotKey], Storage.List[CurrentSlotKey].Stack, Storage, Main.PlayerInfo.position);
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
        ItemSlot itemSlot = Storage.List[CurrentSlotKey];
        if (itemSlot.isEmpty()) return;

        // Shift+right-click splits half; plain right-click takes one onto the cursor.
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (GUICursor.Data.isEmpty() || itemSlot.isSame(GUICursor.Data))
            {
                GUICursor.Data.Add(itemSlot, itemSlot.Stack / 2);
                Audio.PlaySFX(SfxID.Item);
                Storage.NotifyChanged();
            }
        }
        else if (GUICursor.Data.isEmpty() || itemSlot.isSame(GUICursor.Data))
        {
            GUICursor.Data.Add(itemSlot, 1);
            Audio.PlaySFX(SfxID.Item);
            Storage.NotifyChanged();
        }
    }
    protected override void SetInfoPanel(ItemSlot itemSlot)
    {
        string cost = null;
        if (GUIMain.StorageInv == this && GUIMain.GUICraft is { Mode: TradeMode.Shop } shop && shop.ActiveCraftInfo.WantsItem(itemSlot.ID))
            cost = $"sell for {Trade.Format(Trade.SellPrice(itemSlot.ID) * itemSlot.Stack)} (shift-click)";

        GUIMain.Cursor.SetItemSlotInfo(itemSlot, false, cost);
    }

    /// <summary>Sells the hovering slot's stack to the open shop, when it deals in that item.</summary>
    private bool TrySell(ItemSlot slot)
    {
        if (GUIMain.StorageInv != this || slot.isEmpty()) return false;
        if (GUIMain.GUICraft is not { Mode: TradeMode.Shop } shop || !shop.ActiveCraftInfo.WantsItem(slot.ID)) return false;

        int worth = Trade.SellPrice(slot.ID) * slot.Stack;
        Storage.RemoveItem(slot.ID, slot.Stack, CurrentSlotKey);
        Trade.Pay(Storage, worth);
        return true;
    }
}