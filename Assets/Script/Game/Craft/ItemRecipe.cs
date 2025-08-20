using System.Collections.Generic;
 
public partial class ItemRecipe 
{
    public static Dictionary<ID, ItemRecipe> Dictionary = new Dictionary<ID, ItemRecipe>();

    public int Stack;
    public int Time;
    public Dictionary<ID, int> Ingredients;
    public string[] Modifiers;
 
    public static ItemRecipe GetRecipe(ID stringID)
    {
        if (Dictionary.ContainsKey(stringID))
            return Dictionary[stringID];
        return null;
    }
    
    public static void AddRecipe(ID stringID, Dictionary<ID, int> ingredients, int stack, int time, string[] modifiers)
    {
        Dictionary.Add(stringID, new ItemRecipe()
        {
            Stack = stack,
            Ingredients = ingredients,
            Time = time,
            Modifiers = modifiers,
        });
    }

    public static bool IsCraftable (ID stringID)
    {
        foreach (var ingredient in Dictionary[stringID].Ingredients)
        {
            if (Game.PlayerInfo.Storage.GetAmount(ingredient.Key) < ingredient.Value) return false;
        } 
        return true;
    }
    
    public static void CraftItem(ID stringID, bool isCursor = true)
    {
        TakeIngredients(stringID);
        
        ItemSlot craftedItem = new ItemSlot();
        
        craftedItem.SetItem(Dictionary[stringID].Stack, stringID, null, false);
        if (isCursor && (GUICursor.Data.isEmpty() || GUICursor.Data.isSame(craftedItem)))
        {
            GUICursor.Data.Add(craftedItem);
        }
        
        if (craftedItem.Stack > 0)
        { 
            Game.PlayerInfo.Storage.AddItem(stringID, 1, Game.PlayerInfo.Storage.Key);
            Inventory.RefreshInventory();
        }  
    }

    public static void TakeIngredients(ID stringID)
    {
        Audio.PlaySFX(SfxID.Item);
        foreach (var ingredient in Dictionary[stringID].Ingredients)
        {
            Game.PlayerInfo.Storage.RemoveItem(ingredient.Key, ingredient.Value);
        }
    }
}
