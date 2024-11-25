

public enum ItemRarity{ Common, Rare, Epic, Legendary }
public class ItemData
{
    public string StringID { get; set; }
    public string Name { get; set; }
    public int StackSize { get; set; }
    public ItemRarity Rarity { get; set; }
    public int Damage { get; set; }
    public float Knockback { get; set; }
    public int UseTime { get; set; }
    public string Description { get; set; }
    public bool IsConsumable { get; set; }

    public ItemData(string stringID, string name, int stackSize, ItemRarity rarity, string description, bool isConsumable, int damage, float knockback, int useTime )
    {
        StringID = stringID;
        Name = name;
        StackSize = stackSize;
        Rarity = rarity;
        Damage = damage;
        Knockback = knockback;
        UseTime = useTime;
        Description = description;
        IsConsumable = isConsumable;
    }
}