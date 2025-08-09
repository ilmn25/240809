using System;
using System.Collections.Generic;
using System.IO;

[Serializable]
public class PlayerData
{
    public List<ItemSlot> inventory;
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
        int totalSlots = Inventory.InventorySlotAmount * Inventory.InventoryRowAmount;
        inventory = new List<ItemSlot>(totalSlots);
        for (int i = 0; i < totalSlots; i++)
        {
            inventory.Add(new ItemSlot());
        }
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