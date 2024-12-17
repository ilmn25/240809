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
            if (InventorySingleton.GetAmount(ingredient.Key) < ingredient.Value) return false;
        } 
        return true;
    }
    
    public static void CraftItem(string stringID)
    {
        if (!IsCraftable(stringID)) return;

        foreach (var ingredient in Dictionary[stringID].Ingredients)
        {
            InventorySingleton.RemoveItem(ingredient.Key, ingredient.Value);
        }
        
        InvSlotData craftedItem = new InvSlotData();
        
        craftedItem.SetItem(Dictionary[stringID].Stack, stringID, null, false);
        if (GUICursorSingleton._cursorSlot.isEmpty() || GUICursorSingleton._cursorSlot.isSame(craftedItem))
        {
            GUICursorSingleton._cursorSlot.Add(craftedItem);
        }
        
        if (craftedItem.Stack > 0)
        { 
            InventorySingleton.AddItem(stringID);
        } 
    }
}
