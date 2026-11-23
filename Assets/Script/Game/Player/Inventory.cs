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

    /// <summary>Cursor stack when one is held, otherwise the selected hotbar slot.</summary>
    public static ItemSlot Held =>
        !GUICursor.Data.isEmpty() ? GUICursor.Data : Main.PlayerInfo?.Storage?.GetSelected();

    public static void RefreshHeldItem(MobInfo info)
    {
        if (info is not PlayerInfo player || player.Storage == null) return;

        ItemSlot held = ReferenceEquals(player, Main.PlayerInfo) ? Held : player.Storage.GetSelected();
        player.SetEquipment(held is { Stack: > 0 } ? held : null);
    }

    /// <summary>Replace the hand's contents in place, so the equipment keeps pointing at the
    /// same slot when an item transforms (an empty bucket filling up).</summary>
    public static void ReplaceHeld(PlayerInfo player, ID id, int count = 1)
    {
        ItemSlot held = player?.HeldSlot;
        if (held == null || player.Storage == null) return;

        held.clear();
        held.ID = id;
        held.Stack = count;
        held.Durability = Item.GetItem(id).Durability;
        player.Storage.NotifyChanged();
        GUICursor.UpdateCursorSlot();
    }

    private static void SyncCurrentItemState()
    {  
        ItemSlot held = Held;

        if (held is { Stack: > 0 })
        {
            CurrentItem = held;
            CurrentItemData = Item.GetItem(held.ID); 
        }
        else
        {
            CurrentItemData = null;
            CurrentItem = null;
        }

        RefreshHeldItem(Main.PlayerInfo);
    }

    /// <summary>Hand a picked-up drop to the cursor when the owner's bag can't take it.
    /// Only the locally controlled player has a cursor, so everyone else (AI allies, remote
    /// players) keeps the old drop-to-the-ground behavior.</summary>
    public static bool TryPickUpToCursor(PlayerInfo owner, ItemSlot item)
    {
        if (owner == null || !ReferenceEquals(owner, Main.PlayerInfo)) return false;
        if (item == null || item.isEmpty()) return false;
        // The bag takes it when it can; the cursor is only the overflow.
        if (owner.Storage != null && owner.Storage.CanFit(item)) return false;
        // The cursor can only take the drop if it is empty or already holds the same item.
        if (!GUICursor.Data.isEmpty() && !(GUICursor.Data.isSame(item) && !GUICursor.Data.isFull()))
            return false;

        GUICursor.Data.Add(item);
        GUICursor.UpdateCursorSlot();
        RefreshInventory();
        // Anything that didn't fit on the cursor falls back to the usual ground drop.
        if (!item.isEmpty()) owner.Storage?.AddItem(item);
        return true;
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
    public static ItemInfo DropToWorld(ItemSlot slot, int amount, Storage storage, Vector3 position)
    {
        if (Helper.IsHost())
        {
            ItemInfo spawned = Entity.SpawnItem(slot, position, amount: amount);
            storage.NotifyChanged();
            return spawned;
        }
        if (NetworkClient.isConnected)
        {
            ClientDropSlot(slot, amount, storage, position);
        }
        return null;
    }

    public static void Update()
    {
        // Spectating clients cannot use inventory (drop, hotkeys)
        if (!Helper.IsHost() && NetworkClient.isConnected &&
            Main.PlayerInfo != null &&
            !PlayerSync.CanLocalClientControl(Main.PlayerInfo.uid))
            return;

        if (Control.Inst.Drop.KeyDown() && CurrentItem is { Stack: > 0 })
        {
            int dropAmount = Input.GetKey(KeyCode.LeftControl) ? CurrentItem.Stack : 1;
            DropToWorld(CurrentItem, dropAmount, Main.PlayerInfo.Storage, Main.Player.transform.position);
            RefreshInventory();
        }

        if (!Dialogue.ChoicesActive)
        {
            int hotbar = PressedHotbarSlot();
            if (hotbar != -1 && !PlaceHeldInSlot(hotbar))
            {
                _buffer = hotbar;
                RefreshInventory();
            }
        }

        if (Main.PlayerInfo.Machine && Main.PlayerInfo.Machine.IsCurrentState<DefaultState>() &&
            _buffer != -1)
        {
            Audio.PlaySFX(SfxID.Text);
            Main.PlayerInfo.Storage.Key = _buffer;
            _buffer = -1;
            SyncCurrentItemState();
            RefreshInventory();
        }
    }

    private static int PressedHotbarSlot()
    {
        Control control = Control.Inst;
        if (control.Hotbar1.KeyDown()) return 0;
        if (control.Hotbar2.KeyDown()) return 1;
        if (control.Hotbar3.KeyDown()) return 2;
        if (control.Hotbar4.KeyDown()) return 3;
        if (control.Hotbar5.KeyDown()) return 4;
        if (control.Hotbar6.KeyDown()) return 5;
        if (control.Hotbar7.KeyDown()) return 6;
        if (control.Hotbar8.KeyDown()) return 7;
        if (control.Hotbar9.KeyDown()) return 8;
        return -1;
    }

    /// <summary>Move the item held on the cursor into a hotbar slot and select that slot.
    /// An item already in the slot is dropped on the floor. False when the cursor is
    /// empty or the slot holds a full stack of the same item.</summary>
    public static bool PlaceHeldInSlot(int index)
    {
        Storage storage = Main.PlayerInfo?.Storage;
        if (storage?.List == null || GUICursor.Data.isEmpty()) return false;
        if (index < 0 || index >= storage.List.Count) return false;

        ItemSlot target = storage.List[index];
        if (target.isEmpty() || target.isSame(GUICursor.Data) && !target.isFull())
        {
            target.Add(GUICursor.Data);
        }
        else if (target.isSame(GUICursor.Data))
        {
            return false;
        }
        else
        {
            DropToWorld(target, target.Stack, storage, Main.Player.transform.position);
            target.CopyFrom(GUICursor.Data);
            GUICursor.Data.clear();
        }

        _buffer = -1;
        storage.Key = index;
        GUICursor.UpdateCursorSlot();
        storage.NotifyChanged();
        return true;
    }

    public static void HandleScrollInput(float input)
    {
        int step = (int)Mathf.Sign(input);
        int slot = (int)Mathf.Repeat(Main.PlayerInfo.Storage.Key - step, InventorySlotAmount);
        if (PlaceHeldInSlot(slot)) return;

        _buffer = slot;
        RefreshInventory();
    }

    public static void RefreshInventory()
    {  
        SyncCurrentItemState();
        SlotUpdate?.Invoke();  
    }
 
     
}
