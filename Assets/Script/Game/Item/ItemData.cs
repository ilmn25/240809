public enum ItemRarity { Common, Rare, Epic, Legendary }
public enum ItemType { Tool, Armor, Accessory, Block, Furniture }

public enum ItemGesture { Swing, Poke, Cast}
public partial class Item
{
    public string StringID { get; set; }
    public ItemType Type { get; set; }
    public ItemGesture Gesture { get; set; }
    public string Name { get; set; }
    public int StackSize { get; set; }
    public ItemRarity Rarity { get; set; } 
    public float Speed { get; set; }
    public float Range { get; set; }
    public ProjectileInfo ProjectileInfo;
    public int MiningPower { get; set; }
    public string Description { get; set; }
    public bool IsConsumable { get; set; }
    public int HealHP { get; set; }
    public int HealMana { get; set; }

    public Item(
        string stringID,
        string name,
        int stackSize,
        ItemRarity rarity,
        string description,
        bool isConsumable,
        float speed,
        float range,  
        ProjectileInfo projectileInfo,
        int miningPower,
        int healHp,
        int healMana
    )
    {
        StringID = stringID;
        Name = name;
        StackSize = stackSize;
        Rarity = rarity; 
        Speed = speed;
        Range = range;  
        ProjectileInfo = projectileInfo;
        MiningPower = miningPower;
        Description = description;
        IsConsumable = isConsumable;
        HealHP = healHp;
        HealMana = healMana;
    }
}
