using System.Collections.Generic;

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
[System.Serializable]
public class InvSlotData
{
    public int Quantity;
    public string StringID;

    public InvSlotData(string stringID, int quantity)
    {
        StringID = stringID;
        Quantity = quantity; 
    }
}