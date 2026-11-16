using System.Collections.Generic;
using System.Text;
using UnityEngine;
 
public partial class ItemRecipe 
{
    private const string ShortageColor = "#FF5555";
    private const string FulfilledColor = "#8FE38F";

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

    public static List<Storage> Sources()
    {
        List<Storage> sources = new List<Storage>();
        AddSource(sources, Main.PlayerInfo?.Storage);

        if (Main.Player != null && global::Save.Inst?.players != null)
        {
            float reach = ContainerWindow.ProximityRange * ContainerWindow.ProximityRange;
            foreach (PlayerInfo player in global::Save.Inst.players)
            {
                if (player == null || player == Main.PlayerInfo || player.Destroyed ||
                    player.Machine == null || player.PlayerStatus == PlayerStatus.Incapacitated ||
                    !player.HasInventory)
                    continue;
                if (Helper.SquaredDistance(player.Machine.transform.position, Main.Player.transform.position) > reach)
                    continue;
                AddSource(sources, player.Storage);
            }
        }

        if (GUIMain.Storage is { Showing: true })
            AddSource(sources, GUIMain.Storage.Storage);

        return sources;
    }

    private static void AddSource(List<Storage> sources, Storage storage)
    {
        if (storage == null || sources.Contains(storage)) return;
        sources.Add(storage);
    }

    private static int AmountIn(List<Storage> sources, ID item)
    {
        int amount = 0;
        foreach (Storage storage in sources)
            amount += storage.GetAmount(item);
        return amount;
    }

    public static int AvailableAmount(ID item) => AmountIn(Sources(), item);

    public bool IsCraftable()
    {
        if (Main.CreativeMode) return true;

        List<Storage> sources = Sources();
        foreach (var ingredient in Ingredients)
        {
            if (AmountIn(sources, ingredient.Key) < ingredient.Value) return false;
        }
        return true;
    }

    public string DescribeIngredients(bool colored = false)
    {
        if (Ingredients == null || Ingredients.Count == 0) return "";

        List<Storage> sources = Sources();
        StringBuilder text = new StringBuilder("Ingredients:");
        foreach (var ingredient in Ingredients)
        {
            Item item = Item.GetItem(ingredient.Key);
            int have = AmountIn(sources, ingredient.Key);
            string line = $"{item.Name} ({have}/{ingredient.Value})";
            text.Append('\n').Append(colored
                ? $"<color={(have < ingredient.Value ? ShortageColor : FulfilledColor)}>{line}</color>"
                : line);
        }
        return text.ToString();
    }
    
    public static void CraftItem(ID stringID)
    {
        Tutorial.OnCraft(stringID);
        TakeIngredients(stringID);
        GiveItem(stringID, Dictionary[stringID].Stack);
    }

    public static void GiveItem(ID stringID, int stack)
    {
        ItemSlot craftedItem = new ItemSlot(stringID, stack);
        
        if (GUICursor.Data.isEmpty() || GUICursor.Data.isSame(craftedItem))
        {
            GUICursor.Data.Add(craftedItem);
            Audio.PlaySFX(SfxID.Item);
            GUICursor.UpdateCursorSlot();
        }
        
        if (craftedItem.Stack <= 0) return;

        Main.PlayerInfo.Storage.AddItem(craftedItem, Main.PlayerInfo.Storage.Key);
    }

    public static void TakeIngredients(ID stringID)
    {
        Audio.PlaySFX(SfxID.Item);
        if (Main.CreativeMode) return;

        List<Storage> sources = Sources();
        foreach (var ingredient in Dictionary[stringID].Ingredients)
        {
            int missing = ingredient.Value;
            foreach (Storage storage in sources)
            {
                missing = storage.RemoveItem(ingredient.Key, missing);
                if (missing <= 0) break;
            }
        }
    }
}
