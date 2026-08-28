using System;
using System.Collections.Generic;

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
            if (_item == null || _item.ID != ID)
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
    
    private string IngredientText(KeyValuePair<ID, int> ingredient)
    {
        int have = Main.PlayerInfo?.Storage?.GetAmount(ingredient.Key) ?? 0;
        return $"{Item.GetItem(ingredient.Key).Name} ({have}/{ingredient.Value})";
    }

    public string ToString(bool ingredients)
    {
        string text = "";
        if (Info.Type == ItemType.Structure)
        {
            text += "structure";
            ItemRecipe recipe = ItemRecipe.GetRecipe(ID);
            text += " \ningredients: ";
            if (recipe != null)
                foreach (var ingredient in recipe.Ingredients)
                    text += "\n" + IngredientText(ingredient);
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
                        text += "\n" + IngredientText(ingredient);
                }
            }

            text += "\n \n" + Info.Description;
        }
        else if (Info.Type == ItemType.Consumable)
        {
            text += Stack + "x";

            if (Info.HealValue > 0)
                text += "\n \nrestores " + Info.HealValue + " health";
            if (Info.HungerValue > 0)
                text += "\n \nrestores " + Info.HungerValue + " hunger";
            if (Info.MaxHpBonus > 0)
                text += "\n \npermanently increases max health by " + Info.MaxHpBonus;
            if (Info.MaxHungerBonus > 0)
                text += "\n \npermanently increases max hunger by " + Info.MaxHungerBonus;


            if (ingredients)
            {
                ItemRecipe recipe = ItemRecipe.GetRecipe(ID);
                if (recipe != null)
                {
                    text += " \n \ningredients: ";
                    foreach (var ingredient in recipe.Ingredients)
                        text += "\n" + IngredientText(ingredient);
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
                if (Info.ProjectileInfo.Breaking != 0)
                {
                    switch (Info.ProjectileInfo.OperationType)
                    {
                        case OperationType.Building:
                            text += " \nbuilding " + Info.ProjectileInfo.Breaking;
                            break;
                        case OperationType.Mining:
                            text += " \nmining " + Info.ProjectileInfo.Breaking;
                            break;
                        case OperationType.Cutting:
                            text += " \nbreaking " + Info.ProjectileInfo.Breaking;
                            break;
                    }
                }  

                if (Info.ProjectileInfo.Ammo != ID.Null)
                    text += " \n \nammo: " + (AmmoRegistry.DescribeForGun(ID) ?? Info.ProjectileInfo.Ammo.ToString());
            } 


            if (ingredients)
            {
                text += " \n \ningredients: ";
                ItemRecipe recipe = ItemRecipe.GetRecipe(ID);
                if (recipe != null)
                {
                    foreach (var ingredient in recipe.Ingredients)
                        text += "\n" + IngredientText(ingredient);
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