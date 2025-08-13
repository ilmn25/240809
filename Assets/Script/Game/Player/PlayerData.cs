using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class PlayerData
{
    public Storage inventory;
    public int health = 200;
    public int mana = 100;
    public int sanity = 100;
    public int hunger = 100;
    public int stamina = 100;  
    public int speed = 100;
    public SVector3Int position;

    [NonSerialized]
    public static PlayerData Inst;

    public PlayerData()
    {
        inventory = new Storage( Inventory.InventorySlotAmount * Inventory.InventoryRowAmount);
    }
    
    public static void Save()
    { 
        Inst.inventory = Inventory.Storage;
        Inst.position = new SVector3Int(Vector3Int.FloorToInt(Game.Player.transform.position));
        Utility.FileSave(Inst, "player");
    }

    public static void Load()
    {
        Inst = Utility.FileLoad<PlayerData>("player") ?? new PlayerData();
        Inventory.SetInventory(Inst.inventory);
    }
} 