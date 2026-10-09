using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

[System.Serializable]
public class ItemSlot
{
    public int Stack = 0;
    public int Durability;
    public ID ID;
    public string Modifier;
    public bool Locked;

    [OnDeserialized]
    private void MigrateLegacyIds(StreamingContext context)
    {
        if (ID == ID.CopperChunksLegacy) ID = ID.LeadChunks;
    }
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

    private string IngredientsText(bool ingredients)
    {
        if (!ingredients) return "";
        ItemRecipe recipe = ItemRecipe.GetRecipe(ID);
        if (recipe == null) return "";

        string text = " \n \ningredients: ";
        foreach (var ingredient in recipe.Ingredients)
            text += "\n" + IngredientText(ingredient);
        return text;
    }

    public string ToString(bool ingredients, string cost = null)
    {
        string stats = Info.Type switch
        {
            ItemType.Structure => "structure",
            ItemType.Block or ItemType.Material => Stack + "x",
            ItemType.Consumable => Stack + "x" + ConsumableStats(),
            ItemType.Tool => ToolStats(),
            _ => null,
        };
        if (stats == null) return "";

        string costBlock = cost == null
            ? IngredientsText(ingredients || Info.Type == ItemType.Structure)
            : " \n \n" + cost;
        return stats + costBlock + "\n \n" + Info.Description;
    } 

    private string ConsumableStats()
    {
        string text = "";
        if (Info.HealValue > 0)
            text += "\n \nrestores " + Info.HealValue + " health";
        if (Info.HungerValue > 0)
            text += "\n \nrestores " + Info.HungerValue + " hunger";
        if (Info.MaxHpBonus > 0)
            text += "\n \npermanently increases max health by " + Info.MaxHpBonus;
        if (Info.MaxHungerBonus > 0)
            text += "\n \npermanently increases max hunger by " + Info.MaxHungerBonus;
        return text;
    }

    private string ToolStats()
    {
        string text = "";
        if (Durability != -1) text += Durability + "x\n";

        if (Info.ProjectileInfo == null) return text;

        text += Info.ProjectileInfo.Damage + " damage";
        text += " \n" + Info.ProjectileInfo.Knockback + " knockback\n";
        if (Info.ProjectileInfo.Breaking != 0)
        {
            text += Info.ProjectileInfo.OperationType switch
            {
                OperationType.Building => " \nbuilding " + Info.ProjectileInfo.Breaking,
                OperationType.Mining => " \nmining " + Info.ProjectileInfo.Breaking,
                OperationType.Cutting => " \nbreaking " + Info.ProjectileInfo.Breaking,
                _ => "",
            };
        }

        if (Info.ProjectileInfo.Ammo != ID.Null)
            text += " \n \nammo: " + (AmmoRegistry.DescribeForGun(ID) ?? Info.ProjectileInfo.Ammo.ToString());
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