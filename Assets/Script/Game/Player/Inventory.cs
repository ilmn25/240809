using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Inventory 
{ 
    private static int _buffer = 0;
    public static ItemSlot CurrentItem;
    public static Item CurrentItemData;

    public static readonly int InventoryRowAmount = 1;
    public static readonly int InventorySlotAmount = 9;

    public static event Action SlotUpdate; 

    private static void SyncCurrentItemState()
    {  
        // The cursor item is the held item whenever it isn't empty (inventory open).
        // This single rule lets every place/use path work from the cursor with no
        // separate handling — the cursor item simply becomes the held item.
        ItemSlot held = !GUICursor.Data.isEmpty() ? GUICursor.Data : Main.PlayerInfo.Storage.List[Main.PlayerInfo.Storage.Key];

        if (held is { Stack: > 0 })
        {
            CurrentItem = held;
            CurrentItemData = Item.GetItem(held.ID); 

            Main.PlayerInfo.SetEquipment(held);

            if (CurrentItemData.ID == ID.ChalkPowder || CurrentItem.Info.Type is ItemType.Block or ItemType.Structure)
                Terraform.BlockUpdate(CurrentItem.ID);
            else
                Terraform.BlockUpdate();

            return;
        }

        CurrentItemData = null;
        Terraform.BlockUpdate();
        Main.PlayerInfo.SetEquipment(null);
    }
 
    
    /// <summary>Shared helper: client-side drop. Deducts from the slot, syncs storage,
    /// and tells the server to spawn the world entity via ClientDropItemMessage.</summary>
    public static void ClientDropSlot(ItemSlot slot, int amount, Storage storage, Vector3 position)
    {
        ID itemID = slot.ID;
        slot.Stack -= amount;
        if (slot.Stack <= 0) slot.clear();
        storage.NotifyChanged();
        NetworkClient.Send(new ClientDropItemMessage
        {
            itemID = itemID,
            count = amount,
            position = position
        });
    }

    /// <summary>Drop <paramref name="amount"/> of <paramref name="slot"/> to the world at
    /// <paramref name="position"/>. Host spawns locally; client sends a drop message.</summary>
    public static void DropToWorld(ItemSlot slot, int amount, Storage storage, Vector3 position)
    {
        if (Helper.IsHost())
        {
            Entity.SpawnItem(slot, position, amount: amount);
            storage.NotifyChanged();
        }
        else if (NetworkClient.isConnected)
        {
            ClientDropSlot(slot, amount, storage, position);
        }
    }

    public static void Update()
    {
        // Spectating clients cannot use inventory (drop, hotkeys)
        if (!Helper.IsHost() && NetworkClient.isConnected &&
            Main.PlayerInfo != null &&
            !PlayerSync.CanLocalClientControl(Main.PlayerInfo.uid))
            return;

        if (Control.Inst.Drop.KeyDown() && CurrentItem.Stack != 0)
        {
            int dropAmount = Input.GetKey(KeyCode.LeftControl) ? CurrentItem.Stack : 1;
            DropToWorld(CurrentItem, dropAmount, Main.PlayerInfo.Storage, Main.Player.transform.position);
            RefreshInventory();
        }

        if (Control.Inst.Hotbar1.KeyDown())
        {  
            _buffer = 0;
            RefreshInventory();
        }
        else if (Control.Inst.Hotbar2.KeyDown())
        {  
            _buffer = 1;
            RefreshInventory();
        }
        else if (Control.Inst.Hotbar3.KeyDown())
        {  
            _buffer = 2;
            RefreshInventory();
        }
        else if (Control.Inst.Hotbar4.KeyDown())
        {  
            _buffer = 3;
            RefreshInventory();
        }
        else if (Control.Inst.Hotbar5.KeyDown())
        {  
            _buffer = 4;
            RefreshInventory();
        }
        else if (Control.Inst.Hotbar6.KeyDown())
        {  
            _buffer = 5;
            RefreshInventory();
        }
        else if (Control.Inst.Hotbar7.KeyDown())
        {  
            _buffer = 6;
            RefreshInventory();
        }
        else if (Control.Inst.Hotbar8.KeyDown())
        {  
            _buffer = 7;
            RefreshInventory();
        }
        else if (Control.Inst.Hotbar9.KeyDown())
        {  
            _buffer = 8;
            RefreshInventory();
        }
         

        if (Main.PlayerInfo.Machine && Main.PlayerInfo.Machine.IsCurrentState<DefaultState>())
        {
            if (_buffer != -1)
            {
                Audio.PlaySFX(SfxID.Text);
                Main.PlayerInfo.Storage.Key = _buffer;
                _buffer = -1;
                SyncCurrentItemState();
                RefreshInventory();
            }
        }
    }

    public static void HandleScrollInput(float input)
    {
        _buffer = (int)Mathf.Repeat(_buffer + (int)input, InventorySlotAmount); 
        RefreshInventory(); 
    }

    public static void RefreshInventory()
    {  
        SyncCurrentItemState();
        SlotUpdate?.Invoke();  
    }
 
     
}
