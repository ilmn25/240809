using UnityEngine;

public enum ItemRarity { Common, Rare, Epic, Legendary }
public enum ItemType { Tool, Armor, Accessory, Block, Structure, Material}

public enum ItemGesture { Swing, Poke, Cast, Shoot}
public partial class Item
{
    public ID StringID; 
    public int StackSize;
    public ItemRarity Rarity; 
    
    public float Scale; 
    public string Sfx; 
    
    public ItemType Type; 
    public ItemGesture Gesture;
    
    public float Speed;
    public float Range;
    public ProjectileInfo ProjectileInfo;
    public int Durability;
    public StatusEffect StatusEffect; 
    public float ProjectileOffset;
    public Vector2 HoldoutOffset;
    public float RotationOffset = 90;
     
    public string Name => GetName(); 
    public string Description;

    private string GetName()
    {
        string rawName = StringID.ToString();
        return System.Text.RegularExpressions.Regex.Replace(rawName, "(?<!^)([A-Z])", " $1").ToLower();
    }

}