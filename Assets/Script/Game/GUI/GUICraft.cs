using Mirror;
using UnityEngine;

public class GUICraft : GUIStorage
{
    public Storage DefaultStorage;
    public CraftInfo ActiveCraftInfo { get; private set; }
    public bool IsShop { get; private set; }

    public void UseDefaultStorage()
    {
        ActiveCraftInfo = null;
        IsShop = false;
        Storage = DefaultStorage;
    }

    public void UseCraftingInfo(CraftInfo craftInfo)
    {
        ActiveCraftInfo = craftInfo;
        IsShop = false;
        Storage = craftInfo.GetStoragePool();
    }

    public void UseShop(CraftInfo shop)
    {
        ActiveCraftInfo = shop;
        IsShop = true;
        Storage = shop.GetStoragePool();
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

        // A shop sells its stock for coins; a station crafts from the recipe.
        if (IsShop)
        {
            Buy(item);
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

    protected void SyncCraftStorage()
    {
        if (ActiveCraftInfo == null) return;
        StorageSync.SendCraftUpdate(ActiveCraftInfo.uid, Storage, ActiveCraftInfo.Pending);
    }

    protected override void SetInfoPanel(ItemSlot itemSlot)
    {
        if (IsShop)
        {
            string cost = $"price: {Trade.Format(Trade.Worth(itemSlot.ID))} " +
                          $"(you have {Trade.Format(Trade.Count(Main.PlayerInfo?.Storage))})";
            GUIMain.Cursor.SetItemSlotInfo(itemSlot, false, cost);
            return;
        }

        GUIMain.Cursor.SetItemSlotInfo(itemSlot, ShowIngredients(itemSlot.ID));
    }

    private static bool ShowIngredients(ID id)
    {
        return ItemRecipe.GetRecipe(id) != null;
    }
}
