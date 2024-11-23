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
    public List<KeyValuePair<int, ItemData>> _playerInventory;
    private BinaryFormatter _bf = new BinaryFormatter();

    void Start()
    {
        Instance = this;
        LoadPlayerData();
        // PrintPlayerData();
        // AddItem(3, 1000);
    }

    void OnApplicationQuit()
    {
        SavePlayerData();
    }











    public void AddItem(int numberID, int quantity = 1)
    {
        var kvp = _playerInventory.Find(kvp => kvp.Value.StringID == ItemLoadStatic.ConvertID(numberID));
        if (kvp.Value != null)
        {
            kvp.Value.StackSize += quantity;
        }
        else
        {
            ItemData newItemData = ItemLoadStatic.GetItem(numberID);
            if (newItemData != null)
            {
                int slotID = GetSmallestAvailableSlotID();
                _playerInventory.Add(new KeyValuePair<int, ItemData>(
                    slotID, // Assign smallest non-existing slot ID
                    new ItemData(
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
                    )
                ));
            }
        }
        // PrintPlayerData();
    }


    public void RemoveItem(int numberID, int quantity = 1)
    {
        var kvp = _playerInventory.Find(kvp => kvp.Value.StringID == ItemLoadStatic.ConvertID(numberID));
        if (kvp.Value != null)
        {
            kvp.Value.StackSize -= quantity;
            if (kvp.Value.StackSize <= 0)
            {
                _playerInventory.Remove(kvp);
            }
            SavePlayerData();
        }
    }




    //! UTILITY
    private int GetSmallestAvailableSlotID()
    {
        int slotID = 0;
        while (_playerInventory.Exists(kvp => kvp.Key == slotID))
        {
            slotID++;
        }
        return slotID;
    } 

    public bool HasItem(int numberID)
    {
        return _playerInventory.Exists(kvp => kvp.Value.StringID == ItemLoadStatic.ConvertID(numberID));
    }



















    //! SAVE LOAD
    // private void SavePlayerData()
    // {
    //     string json = JsonUtility.ToJson(_playerInventory);
    //     string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", "PlayerData.json");
    //     File.WriteAllText(path, json);
    //     // Debug.Log("PlayerData saved to Downloads.");
    // }

    // private void LoadPlayerData()
    // {
    //     string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", "PlayerData.json");
    //     if (File.Exists(path))
    //     {
    //         string json = File.ReadAllText(path);
    //         _playerData = JsonUtility.FromJson<PlayerData>(json); 
    //         _playerInventory = _playerData?.inventory ?? new List<KeyValuePair<int, Item>>();
    //         CustomLibrary.Log(_playerData);
    //         CustomLibrary.Log(_playerData.inventory);
    //     }
    //     else
    //     {
    //         Debug.Log("No playerData file found in Downloads.");
    //         _playerInventory = new List<KeyValuePair<int, Item>>(); // Initialize if file does not exist
    //     }
    // }
    //! SAVE LOAD
    private void SavePlayerData()
    {
        string downloadsPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";
        string filePath = $"{downloadsPath}\\PlayerData.dat"; 
        
        using (FileStream file = File.Create(filePath))
        {
            _bf.Serialize(file, _playerInventory);
        }
    }

    private void LoadPlayerData()
    {
        string downloadsPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";
        string filePath = $"{downloadsPath}\\PlayerData.dat";
        if (File.Exists(filePath))
        {
            using (FileStream file = File.Open(filePath, FileMode.Open))
            {
                _playerInventory = (List<KeyValuePair<int, ItemData>>)_bf.Deserialize(file);
            }
        }
        else
        {
            Debug.Log("No playerData file found in Downloads.");
            _playerInventory = new List<KeyValuePair<int, ItemData>>(); // Initialize if file does not exist
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