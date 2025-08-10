using System.Collections.Generic;
 
public partial class ItemRecipe 
{
    public static Dictionary<string, ItemRecipe> Dictionary = new Dictionary<string, ItemRecipe>();

    public int Stack;
    public Dictionary<string, int> Ingredients;
    public string[] Modifiers;
 
    public static ItemRecipe GetRecipe(string stringID)
    {
        return Dictionary[stringID];
    }
    
    public static void AddRecipe(string stringID, Dictionary<string, int> ingredients, int stack, string[] modifiers)
    {
        Dictionary.Add(stringID, new ItemRecipe()
        {
            Stack = stack,
            Ingredients = ingredients,
            Modifiers = modifiers,
        });
    }

    private static bool IsCraftable (string stringID)
    {
        foreach (var ingredient in Dictionary[stringID].Ingredients)
        {
            if (Inventory.Storage.GetAmount(ingredient.Key) < ingredient.Value) return false;
        } 
        return true;
    }
    
    public static void CraftItem(string stringID)
    {
        if (!IsCraftable(stringID)) return;

        foreach (var ingredient in Dictionary[stringID].Ingredients)
        {
            Inventory.RemoveItem(ingredient.Key, ingredient.Value);
        }
        
        ItemSlot craftedItem = new ItemSlot();
        
        craftedItem.SetItem(Dictionary[stringID].Stack, stringID, null, false);
        if (GUICursor.Data.isEmpty() || GUICursor.Data.isSame(craftedItem))
        {
            GUICursor.Data.Add(craftedItem);
        }
        
        if (craftedItem.Stack > 0)
        { 
            Inventory.AddItem(stringID);
        } 
    }
}
