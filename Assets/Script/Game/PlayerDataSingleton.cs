using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Runtime.Serialization.Formatters.Binary;

public class PlayerDataSingleton : MonoBehaviour
{ 
    public static PlayerDataSingleton Instance { get; private set; }  
    public static PlayerData _playerData; 
    private static BinaryFormatter _binaryFormatter = new BinaryFormatter();

    private void Start()
    {
        Instance = this;
        LoadPlayerData();
    }

    private void OnApplicationQuit()
    {
        SavePlayerData();
    }
 

    public static void SavePlayerData()
    { 
        using (FileStream file = File.Create(Game.PLAYER_SAVE_PATH))
        {
            _playerData.inventory = InventorySingleton._playerInventory;
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
            }
        }
        else
        {
            _playerData = new PlayerData(); 
        }
        InventorySingleton._playerInventory = _playerData.inventory;
    }
 
}
