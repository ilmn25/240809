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
        CurrentItem = Main.PlayerInfo.Storage.List[Main.PlayerInfo.Storage.Key];
        if (CurrentItem is { Stack: > 0 })
        {
            CurrentItemData = Item.GetItem(CurrentItem.ID); 

            Main.PlayerInfo.SetEquipment(CurrentItem);

            if (CurrentItemData.ID == ID.Chalk || CurrentItem.Info.Type is ItemType.Block or ItemType.Structure)
                Terraform.BlockUpdate(CurrentItem.ID);
            else
                Terraform.BlockUpdate();

            return;
        }

        CurrentItemData = null;
        Terraform.BlockUpdate();
        Main.PlayerInfo.SetEquipment(null);
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
            if (Helper.IsHost())
            {
                // Host: spawn locally + sync storage
                if (Input.GetKey(KeyCode.LeftControl))
                    Entity.SpawnItem(CurrentItem, Main.Player.transform.position);
                else
                    Entity.SpawnItem(CurrentItem, Main.Player.transform.position, amount: 1);
                Main.PlayerInfo.Storage.NotifyChanged();
            }
            else if (NetworkClient.isConnected)
            {
                // Client: modify local storage, then tell the server to spawn the world entity.
                // Storage.OnChanged → StorageSync.Send() broadcasts the storage change.
                int dropAmount = Input.GetKey(KeyCode.LeftControl) ? CurrentItem.Stack : 1;
                ID itemID = CurrentItem.ID;
                CurrentItem.Stack -= dropAmount;
                if (CurrentItem.Stack <= 0) CurrentItem.clear();
                Main.PlayerInfo.Storage.NotifyChanged();
                NetworkClient.Send(new ClientDropItemMessage
                {
                    itemID = itemID,
                    count = dropAmount,
                    position = Main.PlayerInfo.position
                });
            }
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
