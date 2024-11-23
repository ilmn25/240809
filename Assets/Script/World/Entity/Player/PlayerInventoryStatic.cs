using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventoryStatic : MonoBehaviour
{
    public static PlayerInventoryStatic Instance { get; private set; }  
    

    private int _currentRow = 0;
    private int _currentSlot = 0;

    public int INVENTORY_ROW_AMOUNT = 3; // Example value
    public int INVENTORY_SLOT_AMOUNT = 12; // Example value

    void Start()
    {
        Instance = this;
    }

    void Update()
    {
        HandleInput();  
        HandleSlot();
    }












    private void HandleInput()
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

    private void HandleSlot()
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
    private int CalculateKey(int row = -1, int slot = -1)
    {
        if (row == -1)
        {
            return _currentRow * INVENTORY_SLOT_AMOUNT + _currentSlot;
        }
        return row * INVENTORY_SLOT_AMOUNT + slot;
    }
    
    public ItemData GetItemAtKey(int key = -1)
    {
        int target_key = key == -1 ? CalculateKey() : key;

        if (PlayerDataStatic.Instance._playerInventory.TryGetValue(target_key, out ItemData itemData))
        {
            return itemData;
        }
        return null;
    }














    private void DebugPrintCurrentSlot()
    {
        int key = CalculateKey(_currentRow, _currentSlot);

        if (PlayerDataStatic.Instance._playerInventory.TryGetValue(key, out ItemData itemData))
        {
            Debug.Log($"Row {_currentRow} Slot {_currentSlot}, Key {key} \nItem {itemData.Name} x{itemData.StackSize}");
        }
        else
        {
            Debug.Log($"Row {_currentRow} Slot {_currentSlot}, Key {key} \nNo item");
        }
    }

}
