using UnityEngine;

public class GUICraft : GUIStorage
{
    public Storage DefaultStorage;
    public CraftInfo ActiveCraftInfo { get; private set; }

    public void UseDefaultStorage()
    {
        ActiveCraftInfo = null;
        Storage = DefaultStorage;
    }

    public void UseCraftingInfo(CraftInfo craftInfo)
    {
        ActiveCraftInfo = craftInfo;
        Storage = craftInfo.GetStoragePool();
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
        if (!ItemRecipe.IsCraftable(item.ID))
            return;
 
        if (ItemRecipe.Dictionary[item.ID].Time == 0 || item.Type == ItemType.Structure)
        {
            ItemRecipe.CraftItem(item.ID);
            return;
        }

        if (ActiveCraftInfo == null)
            return;

        ItemRecipe.TakeIngredients(item.ID);
        ActiveCraftInfo.Pending.Add(item.ID);
    }

    protected override void SetInfoPanel(ItemSlot itemSlot)
    {
        GUIMain.Cursor.SetItemSlotInfo(itemSlot, true);
    }
}
