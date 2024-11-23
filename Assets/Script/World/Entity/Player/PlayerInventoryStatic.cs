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
        ItemData itemData = GetItemAtKey();
        if (itemData != null )
        {
            PlayerChunkEditStatic.Instance._blockStringID = itemData.StringID; 
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
    
    public static ItemData GetItemAtKey(int key = -1)
    {
        int target_key = key == -1 ? CalculateKey() : key;

        if (PlayerDataStatic._playerInventory.TryGetValue(target_key, out ItemData itemData))
        {
            return itemData;
        }
        return null;
    }


    public static void AddItem(string stringID, int quantity = 1)
    {
        foreach (var slot in PlayerDataStatic._playerInventory)
        {
            if (slot.Value.StringID == stringID)
            {
                slot.Value.StackSize += quantity;
                return;
            }
        }

        ItemData newItemData = ItemLoadStatic.GetItem(stringID);
        if (newItemData != null)
        {
            int slotID = GetSmallestAvailableSlotID();
            PlayerDataStatic._playerInventory[slotID] = new ItemData(
                newItemData.StringID,
                newItemData.Name,
                quantity,
                newItemData.Rarity,
                newItemData.Description,
                newItemData.IsConsumable,
                newItemData.CraftingMaterials,
                newItemData.Damage,
                newItemData.Knockback,
                newItemData.UseTime
            );
        }
        // PrintPlayerData();
    }

    public static void RemoveItem(string stringID, int quantity = 1)
    {
        foreach (var kvp in PlayerDataStatic._playerInventory)
        {
            if (kvp.Value.StringID == stringID)
            {
                kvp.Value.StackSize -= quantity;
                if (kvp.Value.StackSize <= 0)
                {
                    PlayerDataStatic._playerInventory.Remove(kvp.Key);
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

        if (PlayerDataStatic._playerInventory.TryGetValue(key, out ItemData itemData))
        {
            Debug.Log($"Row {_currentRow} Slot {_currentSlot}, Key {key} \nItem {itemData.Name} x{itemData.StackSize}");
        }
        else
        {
            Debug.Log($"Row {_currentRow} Slot {_currentSlot}, Key {key} \nNo item");
        }
    }

}
