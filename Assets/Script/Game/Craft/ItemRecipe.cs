using System.Collections.Generic;
using System.Text;
using UnityEngine;
 
public partial class ItemRecipe 
{
    private const string ShortageColor = "#FF5555";

    public static readonly Dictionary<ID, ItemRecipe> Dictionary = new Dictionary<ID, ItemRecipe>();

    public int Stack;
    public int Time;
    public Dictionary<ID, int> Ingredients;
    public string[] Modifiers;
 
    public static ItemRecipe GetRecipe(ID stringID) =>
        Dictionary.TryGetValue(stringID, out ItemRecipe recipe) ? recipe : null;
    
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

    /// <summary>How many of an item the player can reach: their own pool plus the storage
    /// open beside the crafting window.</summary>
    public static int AvailableAmount(ID item)
    {
        int available = Main.PlayerInfo?.Storage?.GetAmount(item) ?? 0;
        if (GUIMain.Storage is { Showing: true })
            available += GUIMain.Storage.Storage.GetAmount(item);
        return available;
    }

    public bool IsCraftable()
    {
        // Creative mode: everything is craftable without resources.
        if (Main.CreativeMode) return true;

        foreach (var ingredient in Ingredients)
        {
            if (AvailableAmount(ingredient.Key) < ingredient.Value) return false;
        }
        return true;
    }

    /// <summary>Ingredient list as tooltip lines: "ingredients:" followed by "name (have/need)".
    /// Colored lines turn red while the player is short, otherwise they take the
    /// ingredient's rarity color. Empty when the recipe needs nothing.</summary>
    public string DescribeIngredients(bool colored = false)
    {
        if (Ingredients == null || Ingredients.Count == 0) return "";

        StringBuilder text = new StringBuilder("ingredients:");
        foreach (var ingredient in Ingredients)
        {
            Item item = Item.GetItem(ingredient.Key);
            int have = AvailableAmount(ingredient.Key);
            string line = $"{item.Name} ({have}/{ingredient.Value})";
            text.Append('\n').Append(colored
                ? $"<color={(have < ingredient.Value ? ShortageColor : item.RarityColor)}>{line}</color>"
                : line);
        }
        return text.ToString();
    }
    
    public static void CraftItem(ID stringID, bool isCursor = true)
    {
        Tutorial.OnCraft(stringID);
        TakeIngredients(stringID);
        GiveItem(stringID, Dictionary[stringID].Stack, isCursor);
    }

    public static void GiveItem(ID stringID, int stack, bool isCursor = true)
    {
        ItemSlot craftedItem = new ItemSlot(stringID, stack);
        
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
        // Creative mode: crafting is free, don't consume any resources.
        if (Main.CreativeMode) return;

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
