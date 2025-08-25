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
        string text = "";
        if (Info.Type == ItemType.Structure)
        {
            text += "structure";
            StructureRecipe recipe = StructureRecipe.Dictionary[ID];
            text += " \n \nbuild time: " + recipe.Time + "s";
            text += " \ningredients: ";
            foreach (var ingredient in recipe.Ingredients)
            {
                text += "\n" + Item.GetItem(ingredient.Key).Name + " x " + ingredient.Value;
            }
            text += "\n \n" + Info.Description;
        }
        else 
        if (Info.Type == ItemType.Block || Info.Type == ItemType.Material)
        { 
            text += Stack + "x";

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

            text += "\n \n" + Info.Description;
        }
        else if (Info.Type == ItemType.Tool)
        { 
            if (Durability != -1) text += Durability + "x\n";

            if (Info.ProjectileInfo != null)
            {
                text += Info.ProjectileInfo.Damage + " damage";
                text += " \n" + Info.ProjectileInfo.Knockback + " knockback\n";
                // if (Info.MiningPower != 0) text += " \nmining power: " + Info.MiningPower;  
                if (Info.ProjectileInfo.Breaking != 0)
                {
                    switch (Info.ProjectileInfo.OperationType)
                    {
                        case OperationType.Building:
                            text += " \nbuilding: " + Info.ProjectileInfo.Breaking;
                            break;
                        case OperationType.Mining:
                            text += " \nmining: " + Info.ProjectileInfo.Breaking;
                            break;
                        case OperationType.Cutting:
                            text += " \nbreaking: " + Info.ProjectileInfo.Breaking;
                            break;
                    }
                }  
                // text += " \nattack cooldown: " + Info.Speed;
                // if (Info.ProjectileInfo is RangedProjectileInfo) text += " \nbullet speed: " + Info.ProjectileInfo.Damage;  
                // else text += " \nrange: " + Info.ProjectileInfo.Radius;
                // text += " \ncrit chance: " + Info.ProjectileInfo.CritChance * 100 + "%";  
                if (Info.ProjectileInfo.Ammo != ID.Null) text += " \n \nammo: " + Info.ProjectileInfo.Ammo;
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

            text += "\n \n" + Info.Description;
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
        int maxStackSize = slot.Info.StackSize;
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
        return Stack == Info.StackSize;
    }
     
}