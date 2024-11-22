using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Runtime.Serialization.Formatters.Binary;

public class PlayerDataStatic : MonoBehaviour
{ 
    public static PlayerDataStatic Instance { get; private set; }  
    public static PlayerData _playerData = new PlayerData();
    public static Dictionary<int, ItemData> _playerInventory;
    private static BinaryFormatter _binaryFormatter = new BinaryFormatter();

    private void Start()
    {
        Instance = this;
        LoadPlayerData();
        // PrintPlayerData();
        // AddItem("exampleID", 1000);
    }

    private void OnApplicationQuit()
    {
        SavePlayerData();
    }
 

    public static void SavePlayerData()
    { 
        using (FileStream file = File.Create(Game.PLAYER_SAVE_PATH))
        {
            _binaryFormatter.Serialize(file, _playerInventory);
        }
    }

    public static void LoadPlayerData()
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

    public static void PrintPlayerData()
    {
        foreach (var kvp in _playerInventory)
        {
            Debug.Log("PlayerData contains: "+kvp.Value.Name + " x" + kvp.Value.StackSize + " (Slot ID: " + kvp.Key + ")");
        }
    }
}
