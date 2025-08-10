using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory 
{
    public static Storage Storage; 
    
    private static int _currentRow = 0;
    private static int _currentSlot = 0;
    private static int _currentKey = 0;
    public static ItemSlot CurrentItem;
    public static Item CurrentItemData;

    public static readonly int InventoryRowAmount = 3;
    public static readonly int InventorySlotAmount = 9;

    public static event Action SlotUpdate; 

    public static void SetInventory(Storage storage)
    {
        Storage = storage;
        RefreshInventory();
    }
    
    public static void Update()
    { 
        if (Control.Inst.Drop.KeyDown() && CurrentItem != null)
        {
            Entity.SpawnItem(CurrentItem.StringID, Vector3Int.FloorToInt(Game.Player.transform.position), false);
            RemoveItem(CurrentItem.StringID); 
        }

        if (Control.Inst.Hotbar.KeyDown())
        {
            _currentRow = (_currentRow + 1) % InventoryRowAmount;
            RefreshInventory();
        }

        if (Control.Inst.Hotbar1.KeyDown())
        {  
            _currentSlot = 0;
            RefreshInventory();
        }
        else if (Control.Inst.Hotbar2.KeyDown())
        {  
            _currentSlot = 1;
            RefreshInventory();
        }
        else if (Control.Inst.Hotbar3.KeyDown())
        {  
            _currentSlot = 2;
            RefreshInventory();
        }
        else if (Control.Inst.Hotbar4.KeyDown())
        {  
            _currentSlot = 3;
            RefreshInventory();
        }
        else if (Control.Inst.Hotbar5.KeyDown())
        {  
            _currentSlot = 4;
            RefreshInventory();
        }
        else if (Control.Inst.Hotbar6.KeyDown())
        {  
            _currentSlot = 5;
            RefreshInventory();
        }
        else if (Control.Inst.Hotbar7.KeyDown())
        {  
            _currentSlot = 6;
            RefreshInventory();
        }
        else if (Control.Inst.Hotbar8.KeyDown())
        {  
            _currentSlot = 7;
            RefreshInventory();
        }
        else if (Control.Inst.Hotbar9.KeyDown())
        {  
            _currentSlot = 8;
            RefreshInventory();
        }
    }

    public static void HandleScrollInput(float input)
    {
        _currentSlot = (int)Mathf.Repeat(_currentSlot + (int)input, InventorySlotAmount); 
        RefreshInventory(); 
    }

    public static void RefreshInventory()
    { 
        _currentKey = CalculateKey();
        CurrentItem = Storage.List[_currentKey];
        Item itemData = CurrentItemData;
        CurrentItemData = CurrentItem.Stack == 0 ? null : Item.GetItem(CurrentItem.StringID);
        if (itemData != CurrentItemData)
            SlotUpdate?.Invoke(); 
    }

    public static int CalculateKey(int row = -1, int slot = -1)
    {
        if (row == -1)
            return _currentRow * InventorySlotAmount + _currentSlot;
        return row * InventorySlotAmount + slot;
    } 
    
    public static void AddItem(string stringID, int quantity = 1)
    {  
        Storage.AddItem(stringID, quantity, _currentKey);
        // RefreshInventory();
    }
    
    public static void RemoveItem(string stringID, int quantity = 1)
    {
        Storage.RemoveItem(stringID, quantity, _currentKey);
        // RefreshInventory();
    }   
}
