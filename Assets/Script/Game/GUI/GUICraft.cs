using UnityEngine;

public class GUICraft : GUIStorage
{
    public Storage DefaultStorage;

    public void UseDefaultStorage()
    {
        Storage = DefaultStorage;
    }

    public void UseCraftingStorage(Storage storage)
    {
        Storage = storage;
    }

    private CraftInfo CraftInfo => Storage.info as CraftInfo ?? CreateCraftInfoFallback();

    private CraftInfo CreateCraftInfoFallback()
    {
        CraftInfo craftInfo = new CraftInfo()
        {
            Storage = Storage,
        };
        Storage.info = craftInfo;
        return craftInfo;
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

        ItemRecipe.TakeIngredients(item.ID);

        if (ItemRecipe.Dictionary[item.ID].Time == 0)
        {
            ItemRecipe.CraftItem(item.ID);
            return;
        }

        CraftInfo.Pending.Add(item.ID);
    }

    private void SpawnCraftedItem(ID itemID)
    {
        Vector3 spawnBase = CraftInfo.Machine != null ? CraftInfo.Machine.transform.position : Main.Player.transform.position;
        Vector3 offset = new Vector3(
            Random.value > 0.5f ? 0.65f : -0.65f,
            1.8f,
            Random.value > 0.5f ? 0.65f : -0.65f);

        Entity.SpawnItem(itemID, spawnBase + offset, stackOnSpawn: false);
    }

    protected override void SetInfoPanel(ItemSlot itemSlot)
    {
        GUIMain.Cursor.SetItemSlotInfo(itemSlot, true);
    }
}
