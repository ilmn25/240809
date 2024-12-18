using System.Collections.Generic;
 
public class Craft 
{
    public static Dictionary<string, CraftData> Dictionary = new Dictionary<string, CraftData>();

    public static CraftData GetItem(string stringID)
    {
        return Dictionary[stringID];
    }
    
    public static void AddCraftingDefinition(string stringID, Dictionary<string, int> ingredients, int stack, string[] modifiers)
    {
        Dictionary.Add(stringID, new CraftData(ingredients, stack, modifiers));
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
        
        InvSlot craftedItem = new InvSlot();
        
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
