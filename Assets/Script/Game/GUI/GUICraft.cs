using System.Text;

public class GUICraft : GUIStorage
{
    protected override string PanelSprite => "GUI/Crafting";

    public Storage DefaultStorage;
    public CraftInfo ActiveCraftInfo { get; private set; }
    /// <summary>What clicking a slot does while a window is open. Crafting is the default, so
    /// the player's own pool always crafts.</summary>
    public TradeMode Mode { get; private set; }

    /// <summary>Short name of what a slot click does, shown in the HUD.</summary>
    public string ActionLabel => Mode switch
    {
        TradeMode.Shop => "buy",
        TradeMode.Sell => "sell",
        _ => "craft",
    };

    /// <summary>Details for the hovered recipe.</summary>
    public override string HoveredItemText
    {
        get
        {
            if (CurrentSlot is not { Stack: > 0 } slot) return "";
            if (Mode == TradeMode.Shop)
                return $"{slot.Info.Name}\nprice: {Trade.Format(Trade.Worth(slot.ID))}";
            if (Mode == TradeMode.Sell)
            {
                int carried = Main.PlayerInfo?.Storage?.GetAmount(slot.ID) ?? 0;
                return $"{slot.Info.Name}\nsells for {Trade.Format(Trade.Worth(slot.ID))} (you have {carried})";
            }
            return $"{slot.Info.Name}\n{CraftingDetails(slot)}";
        }
    }

    private static string CraftingDetails(ItemSlot slot)
    {
        ItemRecipe recipe = ItemRecipe.GetRecipe(slot.ID);
        if (recipe == null) return slot.ToString(false);

        StringBuilder text = new StringBuilder(slot.ToString(false));
        text.Append("\n \ningredients:");
        foreach (var ingredient in recipe.Ingredients)
        {
            int available = Main.PlayerInfo?.Storage?.GetAmount(ingredient.Key) ?? 0;
            if (GUIMain.Storage != null && GUIMain.Storage.Showing)
                available += GUIMain.Storage.Storage.GetAmount(ingredient.Key);

            Item ingredientItem = Item.GetItem(ingredient.Key);
            string color = available < ingredient.Value
                ? "#FF5555"
                : RarityColor(ingredientItem.Rarity);
            text.Append($"\n<color={color}>{ingredientItem.Name} ({available}/{ingredient.Value})</color>");
        }

        return text.ToString();
    }

    private static string RarityColor(ItemRarity rarity) => rarity switch
    {
        ItemRarity.Rare => "#55AAFF",
        ItemRarity.Epic => "#C66CFF",
        ItemRarity.Legendary => "#FFAA33",
        _ => "#FFFFFF",
    };

    public void UseDefaultStorage()
    {
        ActiveCraftInfo = null;
        Mode = TradeMode.Craft;
        Storage = DefaultStorage;
    }

    /// <summary>Shows a menu in the given mode: its slots then do whatever that mode says.</summary>
    public void Use(CraftInfo menu, TradeMode mode)
    {
        ActiveCraftInfo = menu;
        Mode = mode;
        Storage = menu.GetStoragePool();
    }

    protected override void ActionPrimaryDown()
    {
        if (Storage.List[CurrentSlotKey].Stack == 0)
            return;

        QueueOrCraftItem();
    }
    
    protected override void ActionSecondaryKey()
    {
        if (Storage.List[CurrentSlotKey].Stack == 0)
            return;

        QueueOrCraftItem();
    }

    private void QueueOrCraftItem()
    {
        Item item = Item.GetItem(Storage.List[CurrentSlotKey].ID);

        // A shop sells its stock for coins; a collector's window buys the clicked item;
        // a station crafts from the recipe.
        if (Mode == TradeMode.Shop)
        {
            Buy(item);
            return;
        }

        if (Mode == TradeMode.Sell)
        {
            Sell(item);
            return;
        }

        if (!ItemRecipe.IsCraftable(item.ID))
            return;

        if (!ItemRecipe.Dictionary.TryGetValue(item.ID, out ItemRecipe recipe))
            return;

        if (recipe.Time == 0 || item.Type == ItemType.Structure)
        {
            ItemRecipe.CraftItem(item.ID);
            SyncCraftStorage();
            return;
        }

        if (ActiveCraftInfo == null)
            return;

        ItemRecipe.TakeIngredients(item.ID);
        ActiveCraftInfo.Pending.Add(item.ID);
        SyncCraftStorage();
    }

    private static void Buy(Item item)
    {
        if (!Trade.TrySpend(Main.PlayerInfo?.Storage, Trade.Worth(item.ID))) return;

        ItemRecipe.GiveItem(item.ID, ItemRecipe.GetRecipe(item.ID)?.Stack ?? 1);
    }

    /// <summary>Sells one of the listed scrap out of the player's inventory, paying out
    /// coins worth it.</summary>
    private static void Sell(Item item)
    {
        Storage player = Main.PlayerInfo?.Storage;
        if (player == null || player.RemoveItem(item.ID, 1) > 0) return;

        Audio.PlaySFX(SfxID.Item);
        Trade.Pay(player, Trade.Worth(item.ID));
    }

    protected void SyncCraftStorage()
    {
        if (ActiveCraftInfo == null) return;
        StorageSync.SendCraftUpdate(ActiveCraftInfo.uid, Storage, ActiveCraftInfo.Pending);
    }

}
