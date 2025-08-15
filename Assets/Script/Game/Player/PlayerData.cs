// using System;
// using System.Collections.Generic;
// using System.IO;
// using UnityEngine;
//
// [Serializable]
// public class PlayerDataHandler
// {
//     public static PlayerDataHandler Inst;
//     [NonSerialized]
//     public List<PlayerData> PlayerData;
//     
//     public static void Save()
//     { 
//         Utility.FileSave(Inst, "player");
//     }
//
//     public static void Load()
//     {
//         Inst = Utility.FileLoad<PlayerDataHandler>("player") ?? new PlayerDataHandler();
//         Inventory.SetInventory(Inst.PlayerData[0].inventory);
//     }
// }
//
// [Serializable]
// public class PlayerData
// {
//     public Storage inventory;
//     public int health = 6;
//     public int mana = 100;
//     public int sanity = 100;
//     public int hunger = 100;
//     public int stamina = 100;  
//     public int speed = 100;
//     public Vector3 position;
//     public int id;
//     
//     public PlayerData()
//     {
//         inventory = new Storage( Inventory.InventorySlotAmount * Inventory.InventoryRowAmount);
//         id = PlayerDataHandler.Inst.PlayerData.Count;
//         PlayerDataHandler.Inst.PlayerData.Add(this);
//     }
// } 