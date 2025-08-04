using System;
using System.Collections.Generic;
using System.IO;

[Serializable]
public class PlayerData
{
    public List<InvSlot> inventory;
    public int health = 100;
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
        inventory = new List<InvSlot>(totalSlots);
        for (int i = 0; i < totalSlots; i++)
        {
            inventory.Add(new InvSlot());
        }
    } 
    
    public static void Save()
    { 
        Inst.inventory = Inventory.Storage;
        Utility.Save(Inst, "player");
    }

    public static void Load()
    {
        Inst = Utility.Load<PlayerData>("player") ?? new PlayerData();
        Inventory.SetInventory(Inst.inventory);
    }
} 