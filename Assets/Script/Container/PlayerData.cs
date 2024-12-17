using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.Serialization;

[System.Serializable]
public class PlayerData
{
    public List<InvSlotData> inventory;
    public int health = 100;
    public int mana = 100;
    public int sanity = 100;
    public int hunger = 100;
    public int stamina = 100;  
    public int speed = 100;

    public static PlayerData playerData;

    public PlayerData()
    {
        int totalSlots = InventorySingleton.InventorySlotAmount * InventorySingleton.InventoryRowAmount;
        inventory = new List<InvSlotData>(totalSlots);
        for (int i = 0; i < totalSlots; i++)
        {
            inventory.Add(new InvSlotData());
        }
    } 
    
    public static void Save()
    { 
        using (FileStream file = File.Create(Game.PlayerSavePath))
        {
            playerData.inventory = InventorySingleton.PlayerInventory;
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
        InventorySingleton.SetInventory(playerData.inventory);
    }

} 