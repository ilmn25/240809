public enum ItemRarity { Common, Rare, Epic, Legendary }
public enum ItemType { Tool, Armor, Accessory, Block, Furniture }

public partial class Item
{
    public string StringID { get; set; }
    public ItemType Type { get; set; }
    public string Name { get; set; }
    public int StackSize { get; set; }
    public ItemRarity Rarity { get; set; }
    public int Damage { get; set; }
    public float Knockback { get; set; }
    public int CritChance { get; set; }
    public int UseTime { get; set; }
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
        int damage,
        float knockback,
        int critChance,
        int useTime,
        int miningPower,
        int healHP,
        int healMana
    )
    {
        StringID = stringID;
        Name = name;
        StackSize = stackSize;
        Rarity = rarity;
        Damage = damage;
        Knockback = knockback;
        CritChance = critChance;
        UseTime = useTime;
        MiningPower = miningPower;
        Description = description;
        IsConsumable = isConsumable;
        HealHP = healHP;
        HealMana = healMana;
    }
}