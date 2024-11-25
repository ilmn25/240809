using System.Collections.Generic;
using UnityEngine.Serialization;

[System.Serializable]
public class PlayerData
{
    public Dictionary<int, InvSlotData> inventory =  new Dictionary<int, InvSlotData>();
    public int health = 100;
    public int mana = 100;
    public int sanity = 100;
    public int hunger = 100;
    public int stamina = 100;  
    public int speed = 100;

    public PlayerData()
    {
    }
} 