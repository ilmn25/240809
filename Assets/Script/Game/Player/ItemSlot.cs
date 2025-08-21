using System;
using UnityEngine.Serialization;

[System.Serializable]
public class ItemSlot
{
    public int Stack = 0;
    public int Durability;
    public ID ID;
    public string Modifier;
    public bool Locked;
    [NonSerialized] private Item _item;
    public Item Info
    {
        get
        {
            if (_item == null || _item.StringID != ID)
                _item = Item.GetItem(ID);
            return _item;
        }
    }
    public ItemSlot(){}
    public ItemSlot(ID id, int count = 1)
    {
        ID = id;
        Stack = count;
        Durability = Item.GetItem(id).Durability;
    }
    public string ToString(bool ingredients)
    {
        Item item = Item.GetItem(ID);
        string text = item.Name;
        if (item.Type == ItemType.Structure)
        {
            text += "\nstructure";
            StructureRecipe recipe = StructureRecipe.Dictionary[ID];
            text += " \n \nbuild time: " + recipe.Time + "s";
            text += " \ningredients: ";
            foreach (var ingredient in recipe.Ingredients)
            {
                text += "\n" + Item.GetItem(ingredient.Key).Name + " x " + ingredient.Value;
            }
            text += "\n \n" + item.Description;
        }
        else 
        if (item.Type == ItemType.Block || item.Type == ItemType.Material)
        { 
            text += " x " + Stack;

            if (ingredients)
            {
                ItemRecipe recipe = ItemRecipe.GetRecipe(ID);
                if (recipe != null)
                {
                    text += " \n \ningredients: ";
                    foreach (var ingredient in recipe.Ingredients)
                    {
                        text += "\n" + Item.GetItem(ingredient.Key).Name + " x " + ingredient.Value;
                    }
                }
            }

            text += "\n \n" + item.Description;
        }
        else if (item.Type == ItemType.Tool)
        { 
            if (Durability != -1) text += " " + Durability + "%";

            if (item.ProjectileInfo != null)
            {
                text += " \n \n" + item.ProjectileInfo.Damage + " damage";
                text += " \n" + item.ProjectileInfo.Knockback + " knockback\n";
                // if (item.MiningPower != 0) text += " \nmining power: " + item.MiningPower;  
                if (item.ProjectileInfo.Breaking != 0)
                {
                    switch (item.ProjectileInfo.OperationType)
                    {
                        case OperationType.Building:
                            text += " \nbuilding: " + item.ProjectileInfo.Breaking;
                            break;
                        case OperationType.Mining:
                            text += " \nmining: " + item.ProjectileInfo.Breaking;
                            break;
                        case OperationType.Breaking:
                            text += " \nbreaking: " + item.ProjectileInfo.Breaking;
                            break;
                    }
                }  
                // text += " \nattack cooldown: " + item.Speed;
                // if (item.ProjectileInfo is RangedProjectileInfo) text += " \nbullet speed: " + item.ProjectileInfo.Damage;  
                // else text += " \nrange: " + item.ProjectileInfo.Radius;
                // text += " \ncrit chance: " + item.ProjectileInfo.CritChance * 100 + "%";  
                if (item.ProjectileInfo.Ammo != ID.Null) text += " \n \nammo: " + item.ProjectileInfo.Ammo;
            } 


            if (ingredients)
            {
                text += " \n \ningredients: ";
                ItemRecipe recipe = ItemRecipe.Dictionary[ID];
                if (recipe != null)
                {
                    foreach (var ingredient in recipe.Ingredients)
                    {
                        text += "\n" + Item.GetItem(ingredient.Key).Name + " x " + ingredient.Value;
                    }
                }
            }

            text += "\n \n" + item.Description;
        }
        return text;
    } 

    public void clear()
    {
        Stack = 0;
        ID = ID.Null;
        Modifier = null;
        Locked = false;
    }
 
    public void Add(ItemSlot slot, int amountToAdd = 0)
    { 
        if (slot.isEmpty()) return; 
        int maxStackSize = Item.GetItem(slot.ID).StackSize;
        int addableAmount;

        if (amountToAdd == 0)
            addableAmount = Math.Min(slot.Stack, maxStackSize - Stack);
        else
            addableAmount = Math.Min(amountToAdd, Math.Min(slot.Stack, maxStackSize - Stack));

        if (isEmpty())
        {
            ID = slot.ID;
            Modifier = slot.Modifier;
            Locked = slot.Locked;
        }

        if (maxStackSize == 1) Durability = slot.Durability;
        Stack += addableAmount;
        slot.Stack -= addableAmount;

        if (slot.Stack == 0) slot.clear();
    }

    public bool isSame(ItemSlot slot)
    {
        return slot.ID == ID && slot.Modifier == Modifier;
    }
    public bool isSame(ID stringID, string modifier)
    {
        return stringID == ID && modifier == Modifier;
    }
    
    public bool isEmpty()
    {
        return Stack == 0;
    }
    public bool isFull()
    {
        return Stack == Item.GetItem(ID).StackSize;
    }
     
}