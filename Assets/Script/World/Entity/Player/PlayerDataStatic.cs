using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Runtime.Serialization.Formatters.Binary;

public class PlayerDataStatic : MonoBehaviour
{ 
    public static PlayerDataStatic Instance { get; private set; }  
    public PlayerData _playerData;
    public Dictionary<int, ItemData> _playerInventory;
    private BinaryFormatter _binaryFormatter = new BinaryFormatter();

    void Start()
    {
        Instance = this;
        LoadPlayerData();
        // PrintPlayerData();
        // AddItem("exampleID", 1000);
    }

    void OnApplicationQuit()
    {
        SavePlayerData();
    }

    public void AddItem(string stringID, int quantity = 1)
    {
        foreach (var kvp in _playerInventory)
        {
            if (kvp.Value.StringID == stringID)
            {
                kvp.Value.StackSize += quantity;
                return;
            }
        }

        ItemData newItemData = ItemLoadStatic.GetItem(stringID);
        if (newItemData != null)
        {
            int slotID = GetSmallestAvailableSlotID();
            _playerInventory[slotID] = new ItemData(
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

    public void RemoveItem(string stringID, int quantity = 1)
    {
        foreach (var kvp in _playerInventory)
        {
            if (kvp.Value.StringID == stringID)
            {
                kvp.Value.StackSize -= quantity;
                if (kvp.Value.StackSize <= 0)
                {
                    _playerInventory.Remove(kvp.Key);
                }
                SavePlayerData();
                return;
            }
        }
    }

    //! UTILITY
    private int GetSmallestAvailableSlotID()
    {
        int slotID = 0;
        while (_playerInventory.ContainsKey(slotID))
        {
            slotID++;
        }
        return slotID;
    } 

    public bool HasItem(string stringID)
    {
        foreach (var kvp in _playerInventory)
        {
            if (kvp.Value.StringID == stringID)
            {
                return true;
            }
        }
        return false;
    }

    //! SAVE LOAD
    private void SavePlayerData()
    { 
        using (FileStream file = File.Create(Game.PLAYER_SAVE_PATH))
        {
            _binaryFormatter.Serialize(file, _playerInventory);
        }
    }

    private void LoadPlayerData()
    {
        if (File.Exists(Game.PLAYER_SAVE_PATH))
        {
            using (FileStream file = File.Open(Game.PLAYER_SAVE_PATH, FileMode.Open))
            {
                _playerInventory = (Dictionary<int, ItemData>)_binaryFormatter.Deserialize(file);
            }
        }
        else
        {
            Debug.Log("No playerData file found in Downloads.");
            _playerInventory = new Dictionary<int, ItemData>(); // Initialize if file does not exist
        }
    }

    //! DEBUG TOOLS
    public void PrintPlayerData()
    {
        foreach (var kvp in _playerInventory)
        {
            Debug.Log("PlayerData contains: "+kvp.Value.Name + " x" + kvp.Value.StackSize + " (Slot ID: " + kvp.Key + ")");
        }
    }
}
