using System.Collections.Generic;

[System.Serializable]
public class PlayerData
{
    public List<KeyValuePair<int, ItemData>> inventory;
    public int health;
    public int sanity;
    public int hunger;

    public PlayerData(List<KeyValuePair<int, ItemData>> itemList)
    {
        inventory = itemList;
    }
}