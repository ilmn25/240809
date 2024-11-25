using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventoryStatic : MonoBehaviour
{
    public static PlayerInventoryStatic Instance { get; private set; }  
    

    private static int _currentRow = 0;
    private static int _currentSlot = 0;

    public static int INVENTORY_ROW_AMOUNT = 3; // Example value
    public static int INVENTORY_SLOT_AMOUNT = 12; // Example value

    void Start()
    {
        Instance = this;
    }

    public void HandleInventoryUpdate()
    {
        HandleInput();  
        HandleSlot();
    }












    private static void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            _currentRow = (_currentRow + 1) % INVENTORY_ROW_AMOUNT;
            DebugPrintCurrentSlot(); 
        }

        // if (Input.mouseScrollDelta.y != 0)
        // { 
        //     _currentSlot = (int)Mathf.Repeat(_currentSlot + (int)Input.mouseScrollDelta.y, INVENTORY_SLOT_AMOUNT); 
        //         DebugPrintCurrentSlot(); 
        // }

        for (int i = 0; i < INVENTORY_SLOT_AMOUNT; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {  
                _currentSlot = i; 
                DebugPrintCurrentSlot(); 
                break;
            }
        }
    }

    private static void HandleSlot()
    { 
        InvSlotData slot = GetItemAtKey();
        if (slot != null )
        {
            PlayerChunkEditStatic.Instance._blockStringID = slot.StringID; 
        } else {
            PlayerChunkEditStatic.Instance._blockStringID = null;
        }
    }









    //! UTILITY
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
        int target_key = key == -1 ? CalculateKey() : key;

        if (PlayerDataStatic._playerInventory.ContainsKey(target_key))
        {
            return PlayerDataStatic._playerInventory[target_key];
        }
        return null;
    }


    public static void AddItem(string stringID, int quantity = 1)
    {
        foreach (var slot in PlayerDataStatic._playerInventory)
        {
            if (slot.Value.StringID == stringID)
            {
                slot.Value.Quantity += quantity;
                PlayerDataStatic.SavePlayerData();
                return;
            }
        }

        int slotID = GetSmallestAvailableSlotID();
        PlayerDataStatic._playerInventory[slotID] = new InvSlotData(stringID, quantity);
        PlayerDataStatic.SavePlayerData();
    }

    public static void RemoveItem(string stringID, int quantity = 1)
    {
        foreach (var slot in PlayerDataStatic._playerInventory)
        {
            if (slot.Value.StringID == stringID)
            {
                slot.Value.Quantity -= quantity;
                if (slot.Value.Quantity <= 0)
                {
                    PlayerDataStatic._playerInventory.Remove(slot.Key);
                }
                PlayerDataStatic.SavePlayerData();
                return;
            }
        }
    }

    //! UTILITY
    private static int GetSmallestAvailableSlotID()
    {
        int slotID = 0;
        while (PlayerDataStatic._playerInventory.ContainsKey(slotID))
        {
            slotID++;
        }
        return slotID;
    } 

    public static bool HasItem(string stringID)
    {
        foreach (var kvp in PlayerDataStatic._playerInventory)
        {
            if (kvp.Value.StringID == stringID)
            {
                return true;
            }
        }
        return false;
    }












    private static void DebugPrintCurrentSlot()
    {
        int key = CalculateKey(_currentRow, _currentSlot);

        if (PlayerDataStatic._playerInventory.TryGetValue(key, out InvSlotData slot))
        {
            Debug.Log($"Row {_currentRow} Slot {_currentSlot}, Key {key} \nItem {slot.StringID} x{slot.Quantity}");
        }
        else
        {
            Debug.Log($"Row {_currentRow} Slot {_currentSlot}, Key {key} \nNo item");
        }
    }

}
