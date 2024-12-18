using System;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
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
    public static PlayerData playerData;

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
        using (FileStream file = File.Create(Game.PlayerSavePath))
        {
            playerData.inventory = Inventory.PlayerInventory;
            Game.BinaryFormatter.Serialize(file, playerData);
        }
    }

    public static void Load()
    {
        if (File.Exists(Game.PlayerSavePath))
        {
            using (FileStream file = File.Open(Game.PlayerSavePath, FileMode.Open))
            {
                playerData = (PlayerData)Game.BinaryFormatter.Deserialize(file);
            }
        }
        else
        {
            playerData = new PlayerData(); 
        }
        Inventory.SetInventory(playerData.inventory);
    }

} 