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

    /// <summary>Details for the hovered item.</summary>
    public override string HoveredItemText
    {
        get
        {
            if (CurrentSlot is not { Stack: > 0 } slot) return "";
            return slot.Tooltip();
        }
    }

    /// <summary>Recipe label: what the hovered stock costs when trading, its recipe when crafting.</summary>
    public override string HoveredItemDetails
    {
        get
        {
            if (CurrentSlot is not { Stack: > 0 } slot) return "";
            return Mode switch
            {
                TradeMode.Shop => $"price: {Trade.Format(Trade.Worth(slot.ID))}",
                TradeMode.Sell =>
                    $"sells for {Trade.Format(Trade.Worth(slot.ID))} (you have {Main.PlayerInfo?.Storage?.GetAmount(slot.ID) ?? 0})",
                _ => ItemRecipe.GetRecipe(slot.ID)?.DescribeIngredients(colored: true) ?? "",
            };
        }
    }

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

    protected override void ActionPrimaryDown() => QueueOrCraftItem();

    protected override void ActionSecondaryKey() => QueueOrCraftItem();

    private void QueueOrCraftItem()
    {
        if (CurrentSlot is not { Stack: > 0 } slot) return;
        Item item = Item.GetItem(slot.ID);

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

        if (!ItemRecipe.Dictionary.TryGetValue(item.ID, out ItemRecipe recipe) || !recipe.IsCraftable())
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
