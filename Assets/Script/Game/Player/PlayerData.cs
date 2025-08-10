using System;
using System.Collections.Generic;
using System.IO;

[Serializable]
public class PlayerData
{
    public Storage inventory;
    public int health = 1000;
    public int mana = 100;
    public int sanity = 100;
    public int hunger = 100;
    public int stamina = 100;  
    public int speed = 100;

    [NonSerialized]
    public static PlayerData Inst;

    public PlayerData()
    {
        inventory = new Storage( Inventory.InventorySlotAmount * Inventory.InventoryRowAmount);
    }
    
    public static void Save()
    { 
        Inst.inventory = Inventory.Storage;
        Utility.FileSave(Inst, "player");
    }

    public static void Load()
    {
        Inst = Utility.FileLoad<PlayerData>("player") ?? new PlayerData();
        Inventory.SetInventory(Inst.inventory);
    }
} 