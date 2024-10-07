using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInvetorySystem : MonoBehaviour
{
    private PlayerDataSystem _playerDataSystem;
    private PlayerTerraformSystem _playerTerraformSystem;

    private int _currentRow = 0;
    private int _currentSlot = 0;

    public int INVENTORY_ROW_AMOUNT = 3; // Example value
    public int INVENTORY_SLOT_AMOUNT = 12; // Example value

    void Start()
    {
        _playerDataSystem = GetComponent<PlayerDataSystem>(); 
        _playerTerraformSystem = GameObject.Find("player").GetComponent<PlayerTerraformSystem>(); 
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
        Item item = GetItemAtKey();
        if (item != null )
        {
            _playerTerraformSystem._blockID = item.Name; 
        } else {
            _playerTerraformSystem._blockID = null;
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

    public Item GetItemAtKey(int key = -1)
    {
        int target_key = key == -1 ? CalculateKey() : key;
    
        var kvp = _playerDataSystem._playerInventory.Find(kvp => kvp.Key == target_key);
        return kvp.Value;
    }















    //! DEBUG TOOLS
    private void DebugPrintCurrentSlot()
    {
        int key = CalculateKey(_currentRow, _currentSlot);

        var kvp = _playerDataSystem._playerInventory.Find(kvp => kvp.Key == key);
        if (kvp.Value != null)
        {
            Debug.Log($"Row {_currentRow} Slot {_currentSlot}, Key {key} \nItem {kvp.Value.Name} x{kvp.Value.StackSize}");
        }
        else
        {
            Debug.Log($"Row {_currentRow} Slot {_currentSlot}, Key {key} \nNo item");
        }
    }
}
