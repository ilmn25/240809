using System;

[System.Serializable]
public class ItemSlot
{
    public int Stack = 0;
    public int Durability;
    public string StringID;
    public string Modifier;
    public bool Locked;

    public string GetItemInfo(bool ingredients)
    {
        Item item = Item.GetItem(StringID);
        string text = item.Name;
        if (item.Type == ItemType.Structure)
        {
            text += "\nstructure";
            StructureRecipe recipe = StructureRecipe.Dictionary[StringID];
            text += " \n \nbuild time: " + recipe.Time + "s";
            text += " \ningredients: ";
            foreach (var ingredient in recipe.Ingredients)
            {
                text += "\n" + ingredient.Key + " x " + ingredient.Value;
            }
            text += "\n \n" + item.Description;
        }
        else 
        if (item.Type == ItemType.Block)
        { 
            text += " x " + Stack;

            if (ingredients)
            {
                ItemRecipe recipe = ItemRecipe.GetRecipe(StringID);
                if (recipe != null)
                {
                    text += " \n \ningredients: ";
                    foreach (var ingredient in recipe.Ingredients)
                    {
                        text += "\n" + ingredient.Key + " x " + ingredient.Value;
                    }
                }
            }

            text += "\n \n" + item.Description;
        }
        else if (item.Type == ItemType.Tool)
        { 
            text += " " + Durability + "%";

            if (item.ProjectileInfo != null)
            {
                text += " \n \n" + item.ProjectileInfo.Damage + " damage";
                text += " \n" + item.ProjectileInfo.Knockback + " knockback\n";
                // if (item.MiningPower != 0) text += " \nmining power: " + item.MiningPower;  
                if (item.ProjectileInfo.Breaking != 0)
                {
                    switch (item.ProjectileInfo.OperationType)
                    {
                        case OperationType.Build:
                            text += " \nbuilding: " + item.ProjectileInfo.Breaking;
                            break;
                        case OperationType.Dig:
                            text += " \nmining: " + item.ProjectileInfo.Breaking;
                            break;
                        case OperationType.Break:
                            text += " \nbreaking: " + item.ProjectileInfo.Breaking;
                            break;
                    }
                }  
                // text += " \nattack cooldown: " + item.Speed;
                // if (item.ProjectileInfo is RangedProjectileInfo) text += " \nbullet speed: " + item.ProjectileInfo.Damage;  
                // else text += " \nrange: " + item.ProjectileInfo.Radius;
                // text += " \ncrit chance: " + item.ProjectileInfo.CritChance * 100 + "%";  
                if (item.ProjectileInfo.Ammo != null) text += " \n \nammo: " + item.ProjectileInfo.Ammo;
            } 


            if (ingredients)
            {
                text += " \n \ningredients: ";
                ItemRecipe recipe = ItemRecipe.Dictionary[StringID];
                if (recipe != null)
                {
                    foreach (var ingredient in recipe.Ingredients)
                    {
                        text += "\n" + ingredient.Key + " x " + ingredient.Value;
                    }
                }
            }

            text += "\n \n" + item.Description;
        }
        return text;
    }
    
    public void SetItem(int stack, string stringID, string modifier, bool locked)
    {
        Stack = stack;
        StringID = stringID;
        Modifier = modifier;
        Locked = locked;
    }

    public void clear()
    {
        Stack = 0;
        StringID = null;
        Modifier = null;
        Locked = false;
    }
 
    public void Add(ItemSlot slot, int amountToAdd = 0)
    {
        Audio.PlaySFX("pick_up", 0.4f);
        if (slot.isEmpty()) return;
        int maxStackSize = Item.GetItem(slot.StringID).StackSize;
        int addableAmount;

        if (amountToAdd == 0)
            addableAmount = Math.Min(slot.Stack, maxStackSize - Stack);
        else
            addableAmount = Math.Min(amountToAdd, Math.Min(slot.Stack, maxStackSize - Stack));

        if (isEmpty())
        {
            StringID = slot.StringID;
            Modifier = slot.Modifier;
            Locked = slot.Locked;
        }
        
        Stack += addableAmount;
        slot.Stack -= addableAmount;

        if (slot.Stack == 0) slot.clear();
    }

    public bool isSame(ItemSlot slot)
    {
        return slot.StringID == StringID && slot.Modifier == Modifier;
    }
    public bool isSame(string stringID, string modifier)
    {
        return stringID == StringID && modifier == Modifier;
    }
    
    public bool isEmpty()
    {
        return Stack == 0;
    }
    public bool isFull()
    {
        return Stack == Item.GetItem(StringID).StackSize;
    }
     
}