using System.Collections.Generic;
using System.Text;
 
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

    private const float CraftReach = 4f;

    public static List<Storage> Sources()
    {
        List<Storage> sources = new List<Storage>();
        AddSource(sources, Main.PlayerInfo?.Storage);

        foreach (PlayerInfo player in Party(true))
            AddSource(sources, player.Storage);

        if (GUIMain.Storage is { Showing: true })
            AddSource(sources, GUIMain.Storage.Storage);

        return sources;
    }

    private static List<Storage> FarSources()
    {
        List<Storage> sources = new List<Storage>();
        foreach (PlayerInfo player in Party(false))
            AddSource(sources, player.Storage);
        return sources;
    }

    private static IEnumerable<PlayerInfo> Party(bool inReach)
    {
        if (global::Save.Inst?.players == null) yield break;

        foreach (PlayerInfo player in global::Save.Inst.players)
        {
            if (player == null || player == Main.PlayerInfo || player.Destroyed ||
                player.Machine == null || player.PlayerStatus == PlayerStatus.Incapacitated ||
                !player.HasInventory || InReach(player) != inReach)
                continue;
            yield return player;
        }
    }

    private static bool InReach(PlayerInfo player)
    {
        if (Main.Player == null) return false;

        return Helper.SquaredDistance(player.Machine.transform.position, Main.Player.transform.position) <=
               CraftReach * CraftReach;
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

    private static bool HasIngredients(List<Storage> sources, Dictionary<ID, int> ingredients)
    {
        foreach (var ingredient in ingredients)
        {
            if (AmountIn(sources, ingredient.Key) < ingredient.Value) return false;
        }
        return true;
    }

    public static int AvailableAmount(ID item) => AmountIn(Sources(), item);

    public bool IsCraftable()
    {
        if (Main.CreativeMode) return true;
        return HasIngredients(Sources(), Ingredients);
    }

    public string DescribeIngredients(bool colored = false)
    {
        if (Ingredients == null || Ingredients.Count == 0) return "";

        List<Storage> sources = Sources();
        List<Storage> farSources = FarSources();
        StringBuilder text = new StringBuilder("Ingredients:");
        foreach (var ingredient in Ingredients)
        {
            Item item = Item.GetItem(ingredient.Key);
            int have = AmountIn(sources, ingredient.Key);
            int far = AmountIn(farSources, ingredient.Key);
            string line = far > 0
                ? $"{item.Name} x {ingredient.Value} ({have} + {far} too far)"
                : $"{item.Name} x {ingredient.Value} ({have})";
            text.Append('\n').Append(colored
                ? $"<color={(have < ingredient.Value ? ShortageColor : FulfilledColor)}>{line}</color>"
                : line);
        }
        return text.ToString();
    }
    
    public static bool CraftNow(ID stringID, CraftInfo station)
    {
        if (!TakeIngredients(stringID)) return false;

        ItemRecipe recipe = Dictionary[stringID];
        if (recipe.Time > 0 && Item.GetItem(stringID).Type != ItemType.Structure && station != null)
        {
            station.Pending.Add(stringID);
            StorageSync.SendCraftUpdate(station.uid, station.GetStoragePool(), station.Pending);
            return true;
        }

        Tutorial.OnCraft(stringID);
        GiveItem(stringID, recipe.Stack);
        return true;
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

    public static bool TakeIngredients(ID stringID)
    {
        if (Main.CreativeMode) return true;

        List<Storage> sources = Sources();
        if (!HasIngredients(sources, Dictionary[stringID].Ingredients)) return false;

        Audio.PlaySFX(SfxID.Item);
        foreach (var ingredient in Dictionary[stringID].Ingredients)
        {
            int missing = ingredient.Value;
            foreach (Storage storage in sources)
            {
                missing = storage.RemoveItem(ingredient.Key, missing);
                if (missing <= 0) break;
            }
        }
        return true;
    }
}
