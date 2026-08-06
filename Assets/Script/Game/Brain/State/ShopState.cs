using System.Collections.Generic;

/// <summary>Opens the merchant's shop through the craft UI. Mirrors InCraftState but the
/// CraftInfo comes from the MerchantMachine's Shop instead of the entity's own Info.</summary>
public class ShopState : State
{
    private CraftInfo _shop;

    public override void OnEnterState()
    {
        Audio.PlaySFX(SfxID.Text);

        _shop = ((MerchantMachine)Machine).Shop;
        Storage storage = _shop.GetStoragePool();
        EnsureSlotCount(storage, GUIMain.GUICraft.SlotAmount);

        GUIMain.GUICraft.UseCraftingInfo(_shop);

        GUIMain.RefreshStorage();
        GUIMain.Show(true);
        GUIMain.GUICraft.Show(true, !GUIMain.Showing);
    }

    public override void OnUpdateState()
    {
        if (!GUIMain.Showing ||
            Helper.SquaredDistance(Main.Player.transform.position, Machine.transform.position) > 36)
        {
            Machine.SetState<DefaultState>();
            return;
        }

        if (GUIMain.GUICraft.ActiveCraftInfo != _shop)
            Machine.SetState<DefaultState>();
    }

    public override void OnExitState()
    {
        if (GUIMain.GUICraft.ActiveCraftInfo == _shop)
        {
            GUIMain.GUICraft.UseDefaultStorage();
            GUIMain.RefreshStorage();
        }

        _shop = null;
    }

    private static void EnsureSlotCount(Storage storage, int targetCount)
    {
        if (storage.List == null)
            storage.List = new List<ItemSlot>(targetCount);

        while (storage.List.Count < targetCount)
            storage.List.Add(new ItemSlot());
    }
}
