using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InvSlotData
{
    public int Stack;
    public string StringID;
    public string Modifier;
    public bool Locked;

    public InvSlotData(string stringID, int stack)
    {
        StringID = stringID;
        Stack = stack; 
    }
}

public class PlayerInventoryStatic : MonoBehaviour
{
    public static PlayerInventoryStatic Instance { get; private set; }  
    
    public static Dictionary<int, InvSlotData> _playerInventory;

    private static int _currentRow = 0;
    private static int _currentSlot = 0;
    private static int _currentKey = 0;
    public static InvSlotData CurrentItem;

    public static int INVENTORY_ROW_AMOUNT = 3; // Example value
    public static int INVENTORY_SLOT_AMOUNT = 9; // Example value

    void Start()
    {
        Instance = this;
        HandleItemUpdate();
    }

    public void HandleInventoryUpdate()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            AddItem("axe");
        }
        HandleInput();   
    }

    private static void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Tilde))
        {
            _currentRow = (_currentRow + 1) % INVENTORY_ROW_AMOUNT;
            HandleItemUpdate();
        }

        if (Input.mouseScrollDelta.y != 0 && !Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.LeftAlt))
        { 
            _currentSlot = (int)Mathf.Repeat(_currentSlot + (int)Input.mouseScrollDelta.y, INVENTORY_SLOT_AMOUNT); 
            HandleItemUpdate();
        }

        for (int i = 0; i < INVENTORY_SLOT_AMOUNT; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {  
                _currentSlot = i;
                HandleItemUpdate();
                break;
            }
        }
    }


    public static void HandleItemUpdate()
    {
        _currentKey = CalculateKey();
        CurrentItem = GetItemAtKey();
        InventoryStateMachine.Instance.HandleItemUpdate();
    }



    private static int CalculateKey(int row = -1, int slot = -1)
    {
        if (row == -1)
        {
            return _currentRow * INVENTORY_SLOT_AMOUNT + _currentSlot;
        }
        return row * INVENTORY_SLOT_AMOUNT + slot;
    }
    
    public static InvSlotData GetItemAtKey(int key = -1)
    {
        int target_key = key == -1 ? _currentKey : key;

        if (_playerInventory.ContainsKey(target_key))
        {
            return _playerInventory[target_key];
        }
        return null;
    }
    
    public static void AddItem(string stringID, int quantity = 1)
    {
        int maxStackSize = ItemLoadStatic.GetItem(stringID).StackSize;

        // First try to add to the current slot
        if (_playerInventory.ContainsKey(_currentKey) && _playerInventory[_currentKey].StringID == stringID && _playerInventory[_currentKey].Stack < maxStackSize)
        {
            int addableAmount = Math.Min(quantity, maxStackSize - _playerInventory[_currentKey].Stack);
            _playerInventory[_currentKey].Stack += addableAmount;
            quantity -= addableAmount;

            if (quantity <= 0)
            {
                PlayerDataStatic.SavePlayerData();
                return;
            }
        }

        // Try to add to existing slots with the same item
        foreach (var slot in _playerInventory)
        {
            if (slot.Value.StringID == stringID && slot.Value.Stack < maxStackSize)
            {
                int addableAmount = Math.Min(quantity, maxStackSize - slot.Value.Stack);
                slot.Value.Stack += addableAmount;
                quantity -= addableAmount;

                if (quantity <= 0)
                {
                    PlayerDataStatic.SavePlayerData();
                    return;
                }
            }
        }

        // If there's still quantity left, find new slots
        while (quantity > 0)
        { 
            int slotID = GetSmallestAvailableSlotID();
            int addableAmount = Math.Min(quantity, maxStackSize);
            _playerInventory[slotID] = new InvSlotData(stringID, addableAmount);
            quantity -= addableAmount;
        }

        PlayerDataStatic.SavePlayerData();
    }

    public static void RemoveItem(string stringID, int quantity = 1)
    {
        // Prioritize current slot
        if (_playerInventory.ContainsKey(_currentKey) && _playerInventory[_currentKey].StringID == stringID)
        {
            int removableAmount = Math.Min(quantity, _playerInventory[_currentKey].Stack);
            _playerInventory[_currentKey].Stack -= removableAmount;
            quantity -= removableAmount;
            if (_playerInventory[_currentKey].Stack <= 0) _playerInventory.Remove(_currentKey);
            if (quantity <= 0)
            {
                PlayerDataStatic.SavePlayerData();
                return;
            }
        }

        // Continue with other slots if necessary
        foreach (var slot in _playerInventory)
        {
            if (slot.Value.StringID == stringID)
            {
                int removableAmount = Math.Min(quantity, slot.Value.Stack);
                slot.Value.Stack -= removableAmount;
                quantity -= removableAmount;
                if (slot.Value.Stack <= 0)
                {
                    _playerInventory.Remove(slot.Key);
                }
                if (quantity <= 0)
                {
                    PlayerDataStatic.SavePlayerData();
                    return;
                }
            }
        }
        PlayerDataStatic.SavePlayerData();
    }

    public static int GetStackAmount(string stringID)
    {
        int count = 0;
        foreach (var slot in _playerInventory)
        {
            if (slot.Value.StringID == stringID)
            { 
                count += slot.Value.Stack;
            }
        }
        return count;
    }

    private static int GetSmallestAvailableSlotID()
    {
        int slotID = 0;
        while (_playerInventory.ContainsKey(slotID))
        {
            slotID++;
        }
        return slotID;
    }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 25;
        style.normal.textColor = Color.white;
        style.alignment = TextAnchor.UpperRight;

        // Starting position for the labels
        float startX = Screen.width - 300;
        float startY = 10;

        string rowText = $"Row {_currentRow}\n";
        for (int i = 0; i < INVENTORY_SLOT_AMOUNT; i++)
        {
            int key = CalculateKey(_currentRow, i);
            if (key == _currentKey) rowText += ">";
            if (_playerInventory.TryGetValue(key, out InvSlotData slot))
            {
                rowText += $"{slot.StringID} {slot.Stack}\n";
            }
            else
            {
                rowText += "Empty\n";
            }
        }

        Rect rect = new Rect(startX, startY, 290, Screen.height - 20); // Adjusting position and size for the row display
        GUI.Label(rect, rowText, style);
    }

}
