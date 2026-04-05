using System.Collections.Generic;
using UnityEngine;
 
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
            int needed = ingredient.Value;
            int available = Main.PlayerInfo.Storage.GetAmount(ingredient.Key);
            
            // Also check opened storage if the chest GUI is visible
            if (GUIMain.Storage != null && GUIMain.Storage.Showing)
            {
                available += GUIMain.Storage.Storage.GetAmount(ingredient.Key);
            }
            
            if (available < needed) return false;
        } 
        return true;
    }
    
    public static void CraftItem(ID stringID, bool isCursor = true)
    {
        TakeIngredients(stringID);
        
        ItemSlot craftedItem = new ItemSlot(stringID, Dictionary[stringID].Stack);
        
        if (isCursor && (GUICursor.Data.isEmpty() || GUICursor.Data.isSame(craftedItem)))
        {
            GUICursor.Data.Add(craftedItem);
            Audio.PlaySFX(SfxID.Item);
            GUICursor.UpdateCursorSlot();
        }
        
        if (craftedItem.Stack > 0)
        { 
            Main.PlayerInfo.Storage.AddItem(craftedItem, Main.PlayerInfo.Storage.Key);
            Inventory.RefreshInventory();
        }  
    }

    public static void TakeIngredients(ID stringID)
    {
        Audio.PlaySFX(SfxID.Item);
        foreach (var ingredient in Dictionary[stringID].Ingredients)
        {
            // Try to remove from player inventory first
            int missing = Main.PlayerInfo.Storage.RemoveItem(ingredient.Key, ingredient.Value);
            
            // If items are still missing, remove from the visible opened storage
            if (missing > 0 && GUIMain.Storage != null && GUIMain.Storage.Showing)
            {
                GUIMain.Storage.Storage.RemoveItem(ingredient.Key, missing);
            }
        }
    }
}
