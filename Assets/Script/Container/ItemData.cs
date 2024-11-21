[System.Serializable]
public class ItemData
{
    public int ID { get; set; }
    public string Name { get; set; }
    public int StackSize { get; set; }
    public string Rarity { get; set; }
    public int Damage { get; set; }
    public float Knockback { get; set; }
    public int UseTime { get; set; }
    public string Description { get; set; }
    public bool IsConsumable { get; set; }
    public string[] CraftingMaterials { get; set; }

    public ItemData(int id, string name, int stackSize, string rarity, int damage, float knockback, int useTime, string description, bool isConsumable, string[] craftingMaterials)
    {
        ID = id;
        Name = name;
        StackSize = stackSize;
        Rarity = rarity;
        Damage = damage;
        Knockback = knockback;
        UseTime = useTime;
        Description = description;
        IsConsumable = isConsumable;
        CraftingMaterials = craftingMaterials;
    }
}