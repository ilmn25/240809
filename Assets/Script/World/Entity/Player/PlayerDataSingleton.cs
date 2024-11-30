using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Runtime.Serialization.Formatters.Binary;

public class PlayerDataSingleton : MonoBehaviour
{ 
    public static PlayerDataSingleton Instance { get; private set; }  
    public static PlayerData _playerData = new PlayerData(); 
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
            _playerData.inventory = PlayerInventorySingleton._playerInventory;
            _binaryFormatter.Serialize(file, _playerData);
        }
    }

    public static void LoadPlayerData()
    {
        if (File.Exists(Game.PLAYER_SAVE_PATH))
        {
            using (FileStream file = File.Open(Game.PLAYER_SAVE_PATH, FileMode.Open))
            {
                _playerData = (PlayerData)_binaryFormatter.Deserialize(file);
                PlayerInventorySingleton._playerInventory = _playerData.inventory;
            }
        }
        else
        {
            Debug.Log("No playerData file found in Downloads.");
            PlayerInventorySingleton._playerInventory = new Dictionary<int, InvSlotData>(); // Initialize if file does not exist
        }
    }
 
}
