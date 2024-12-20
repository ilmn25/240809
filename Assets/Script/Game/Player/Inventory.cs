using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory 
{
    public static List<InvSlot> PlayerInventory;

    private static int _currentRow = 0;
    private static int _currentSlot = 0;
    private static int _currentKey = 0;
    public static InvSlot CurrentItem;

    public static readonly int InventoryRowAmount = 3;
    public static readonly int InventorySlotAmount = 9;

    public static event Action SlotUpdate; 

    public static void SetInventory(List<InvSlot> data)
    {
        PlayerInventory = data;
        RefreshInventory();
    }
    
    public static void Update()
    { 
        if (Control.control.Drop.KeyDown() && CurrentItem != null)
        {
            Entity.SpawnItem(CurrentItem.StringID, Vector3Int.FloorToInt(Game.Player.transform.position), false);
            RemoveItem(CurrentItem.StringID); 
        }

        if (Control.control.Hotbar.KeyDown())
        {
            _currentRow = (_currentRow + 1) % InventoryRowAmount;
            RefreshInventory();
        }

        if (Control.control.Hotbar1.KeyDown())
        {  
            _currentSlot = 0;
            RefreshInventory();
        }
        else if (Control.control.Hotbar2.KeyDown())
        {  
            _currentSlot = 1;
            RefreshInventory();
        }
        else if (Control.control.Hotbar3.KeyDown())
        {  
            _currentSlot = 2;
            RefreshInventory();
        }
        else if (Control.control.Hotbar4.KeyDown())
        {  
            _currentSlot = 3;
            RefreshInventory();
        }
        else if (Control.control.Hotbar5.KeyDown())
        {  
            _currentSlot = 4;
            RefreshInventory();
        }
        else if (Control.control.Hotbar6.KeyDown())
        {  
            _currentSlot = 5;
            RefreshInventory();
        }
        else if (Control.control.Hotbar7.KeyDown())
        {  
            _currentSlot = 6;
            RefreshInventory();
        }
        else if (Control.control.Hotbar8.KeyDown())
        {  
            _currentSlot = 7;
            RefreshInventory();
        }
        else if (Control.control.Hotbar9.KeyDown())
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
        CurrentItem = PlayerInventory[_currentKey];
        SlotUpdate?.Invoke();
        GUIStorage.RefreshCursorSlot();
    }

    public static int CalculateKey(int row = -1, int slot = -1)
    {
        if (row == -1)
            return _currentRow * InventorySlotAmount + _currentSlot;
        return row * InventorySlotAmount + slot;
    } 
    
    public static void AddItem(string stringID, int quantity = 1)
    {
        int maxStackSize = Item.GetItem(stringID).StackSize;

        // First try to add to the current slot
        if (PlayerInventory[_currentKey].StringID == stringID && PlayerInventory[_currentKey].Stack < maxStackSize)
        {
            int addableAmount = Math.Min(quantity, maxStackSize - PlayerInventory[_currentKey].Stack);
            PlayerInventory[_currentKey].Stack += addableAmount;
            quantity -= addableAmount;

            if (quantity <= 0)
            {
                RefreshInventory();
                return;
            }
        }

        // Try to add to existing slots with the same item
        foreach (var slot in PlayerInventory)
        {
            if (slot.StringID == stringID && slot.Stack < maxStackSize)
            {
                int addableAmount = Math.Min(quantity, maxStackSize - slot.Stack);
                slot.Stack += addableAmount;
                quantity -= addableAmount;

                if (quantity <= 0)
                { 
                    RefreshInventory();
                    return;
                }
            }
        }

        // If there's still quantity left, find new slots
        while (quantity > 0)
        { 
            int slotID = GetEmptySlot();
            int addableAmount = Math.Min(quantity, maxStackSize - PlayerInventory[slotID].Stack);
            PlayerInventory[slotID].SetItem(PlayerInventory[slotID].Stack + addableAmount, stringID, PlayerInventory[slotID].Modifier, PlayerInventory[slotID].Locked);
            quantity -= addableAmount;
        }

        RefreshInventory();
    }
    
    public static void RemoveItem(string stringID, int quantity = 1)
    {
        // Prioritize current slot
        if (PlayerInventory[_currentKey].StringID == stringID)
        {
            int removableAmount = Math.Min(quantity, PlayerInventory[_currentKey].Stack);
            PlayerInventory[_currentKey].Stack -= removableAmount;
            quantity -= removableAmount;
            if (PlayerInventory[_currentKey].Stack <= 0) PlayerInventory[_currentKey].clear();
            if (quantity <= 0)
            { 
                RefreshInventory();
                return;
            }
        }

        // Continue with other slots if necessary
        foreach (var slot in PlayerInventory)
        {
            if (slot.StringID == stringID)
            {
                int removableAmount = Math.Min(quantity, slot.Stack);
                slot.Stack -= removableAmount;
                quantity -= removableAmount;
                if (slot.Stack <= 0)
                {
                    slot.clear();
                }
                if (quantity <= 0)
                {
                    RefreshInventory();
                    return;
                }
            }
        } 
        RefreshInventory();
    }
    
    public static int GetAmount(string stringID)
    {
        int count = 0;
        foreach (var slot in PlayerInventory)
        {
            if (slot.StringID == stringID)
            { 
                count += slot.Stack;
            }
        }
        return count;
    }

    private static int GetEmptySlot()
    {
        int slotID = 0;
        while (PlayerInventory[slotID].Stack != 0)
        {
            slotID++;
        }
        return slotID;
    }
    
    // void OnGUI()
    // {
    //     GUIStyle style = new GUIStyle();
    //     style.fontSize = 25;
    //     style.normal.textColor = Color.white;
    //     style.alignment = TextAnchor.UpperRight;
    //
    //     // Starting position for the labels
    //     float startX = Screen.width - 300;
    //     float startY = 10;
    //
    //     string rowText = $"Row {_currentRow}\n";
    //     for (int i = 0; i < INVENTORY_SLOT_AMOUNT; i++)
    //     {
    //         int key = CalculateKey(_currentRow, i);
    //         if (key == _currentKey) rowText += ">";
    //         if (_playerInventory.TryGetValue(key, out InvSlotData slot))
    //         {
    //             rowText += $"{slot.StringID} {slot.Stack}\n";
    //         }
    //         else
    //         {
    //             rowText += "Empty\n";
    //         }
    //     }
    //
    //     Rect rect = new Rect(startX, startY, 290, Screen.height - 20); // Adjusting position and size for the row display
    //     GUI.Label(rect, rowText, style);
    // }
}
