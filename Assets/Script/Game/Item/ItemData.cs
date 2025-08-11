using UnityEngine;

public enum ItemRarity { Common, Rare, Epic, Legendary }
public enum ItemType { Tool, Armor, Accessory, Block, Structure }

public enum ItemGesture { Swing, Poke, Cast, Shoot}
public partial class Item
{
    public string StringID; 
    public int StackSize;
    public ItemRarity Rarity; 
    
    public float Scale; 
    public string Sfx; 
    
    public ItemType Type; 
    public ItemGesture Gesture;
    
    public float Speed;
    public float Range;
    public ProjectileInfo ProjectileInfo;
    public string Ammo;
    public int MiningPower; 
    public int Durability;
    public StatusEffect StatusEffect; 
    public float ProjectileOffset;
    public float HoldoutOffset;
     
    public string Name; 
    public string Description;  
} 