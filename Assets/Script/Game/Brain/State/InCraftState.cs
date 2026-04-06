public class InCraftState : State
{
    private CraftInfo _craftInfo;

    public override void OnEnterState()
    {
        Audio.PlaySFX(SfxID.Text);

        _craftInfo = ((CraftingMachine)Machine).GetCraftInfo();
        CraftInfo craftInfo = _craftInfo;
        EnsureSlotCount(craftInfo.Storage, GUIMain.GUICraft.SlotAmount);

        GUIMain.GUICraft.UseCraftingStorage(craftInfo.Storage);

        GUIMain.RefreshStorage(); 
        GUIMain.Show(true);
        GUIMain.GUICraft.Show(true, !GUIMain.Showing);
    }

    public override void OnUpdateState()
    {
        CraftInfo craftInfo = _craftInfo ?? ((CraftingMachine)Machine).GetCraftInfo();

        if (!GUIMain.Showing ||
            Helper.SquaredDistance(Main.Player.transform.position, Machine.transform.position) > 36)
        {
            Machine.SetState<DefaultState>();
            return;
        }

        if (GUIMain.GUICraft.Storage != craftInfo.Storage)
            Machine.SetState<DefaultState>();
    }

    public override void OnExitState()
    {
        CraftInfo craftInfo = _craftInfo ?? ((CraftingMachine)Machine).GetCraftInfo();

        if (GUIMain.GUICraft.Storage == craftInfo.Storage)
        {
            GUIMain.GUICraft.UseDefaultStorage();
            GUIMain.RefreshStorage();
        }

        _craftInfo = null;
    }

    private static void EnsureSlotCount(Storage storage, int targetCount)
    {
        if (storage.List == null)
            storage.List = new System.Collections.Generic.List<ItemSlot>(targetCount);

        while (storage.List.Count < targetCount)
            storage.List.Add(new ItemSlot());
    }
}
