using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory 
{ 
    private static int _buffer = 0;
    public static ItemSlot CurrentItem;
    public static Item CurrentItemData;

    public static readonly int InventoryRowAmount = 1;
    public static readonly int InventorySlotAmount = 9;

    public static event Action SlotUpdate; 
 
    
    public static void Update()
    { 
        if (Control.Inst.Drop.KeyDown() && CurrentItem.Stack != 0)
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                Entity.SpawnItem(CurrentItem.StringID, Game.Player.transform.position, CurrentItem.Stack); 
                Game.PlayerInfo.Storage.RemoveItem(CurrentItem.StringID, CurrentItem.Stack, Game.PlayerInfo.Storage.Key);
            }
            else
            {
                Entity.SpawnItem(CurrentItem.StringID, Game.Player.transform.position); 
                Game.PlayerInfo.Storage.RemoveItem(CurrentItem.StringID, 1, Game.PlayerInfo.Storage.Key);
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
         

        if (Game.PlayerInfo.Machine && Game.PlayerInfo.Machine.IsCurrentState<DefaultState>())
        {
            if (_buffer != -1)
            {
                Game.PlayerInfo.Storage.Key = _buffer;
                _buffer = -1;
            }
            CurrentItem = Game.PlayerInfo.Storage.List[Game.PlayerInfo.Storage.Key];
            if (CurrentItem is not { Stack: 0 })
            {
                CurrentItemData = Item.GetItem(CurrentItem.StringID);
                Game.PlayerInfo.SetEquipment(CurrentItem.StringID);
            } 
        }
        if (CurrentItem is { Stack: 0 })
        {
            CurrentItemData = null;     
            Game.PlayerInfo.SetEquipment(ID.Null);
        }
    }

    public static void HandleScrollInput(float input)
    {
        _buffer = (int)Mathf.Repeat(_buffer + (int)input, InventorySlotAmount); 
        RefreshInventory(); 
    }

    public static void RefreshInventory()
    {  
        SlotUpdate?.Invoke();  
    }
 
    
    public static void AddItem(ID stringID, int quantity = 1)
    {  
        Game.PlayerInfo.Storage.AddItem(stringID, quantity, Game.PlayerInfo.Storage.Key, Game.Player.transform.position);
        RefreshInventory();
    }
     
}
