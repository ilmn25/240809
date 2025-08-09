using System.Collections.Generic;
 
public partial class Craft 
{
    public static Dictionary<string, Craft> Dictionary = new Dictionary<string, Craft>();

    public static Craft GetItem(string stringID)
    {
        return Dictionary[stringID];
    }
    
    public static void AddCraftingDefinition(string stringID, Dictionary<string, int> ingredients, int stack, string[] modifiers)
    {
        Dictionary.Add(stringID, new Craft(ingredients, stack, modifiers));
    }

    private static bool IsCraftable (string stringID)
    {
        foreach (var ingredient in Dictionary[stringID].Ingredients)
        {
            if (Inventory.GetAmount(ingredient.Key) < ingredient.Value) return false;
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
        
        InvSlot craftedInv = new InvSlot();
        
        craftedInv.SetItem(Dictionary[stringID].Stack, stringID, null, false);
        if (GUICursor.Data.isEmpty() || GUICursor.Data.isSame(craftedInv))
        {
            GUICursor.Data.Add(craftedInv);
        }
        
        if (craftedInv.Stack > 0)
        { 
            Inventory.AddItem(stringID);
        } 
    }
}
