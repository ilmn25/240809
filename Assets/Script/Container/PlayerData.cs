using System.Collections.Generic;
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

    public PlayerData()
    {
        int totalSlots = InventorySingleton.INVENTORY_SLOT_AMOUNT * InventorySingleton.INVENTORY_ROW_AMOUNT;
        inventory = new List<InvSlotData>(totalSlots);
        for (int i = 0; i < totalSlots; i++)
        {
            inventory.Add(new InvSlotData());
        }
    }
} 