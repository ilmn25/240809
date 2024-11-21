using System.Collections.Generic;

[System.Serializable]
public class PlayerData
{
    public List<KeyValuePair<int, ItemData>> inventory;

    public PlayerData(List<KeyValuePair<int, ItemData>> itemList)
    {
        inventory = itemList;
    }
}