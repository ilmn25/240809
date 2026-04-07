public class InCraftState : State
{
    private CraftInfo _craftInfo;

    public override void OnEnterState()
    {
        Audio.PlaySFX(SfxID.Text);

        _craftInfo = (CraftInfo)Info;
        Storage storage = _craftInfo.GetStoragePool();
        EnsureSlotCount(storage, GUIMain.GUICraft.SlotAmount);

        GUIMain.GUICraft.UseCraftingInfo(_craftInfo);

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

        if (GUIMain.GUICraft.ActiveCraftInfo != _craftInfo)
            Machine.SetState<DefaultState>();
    }

    public override void OnExitState()
    {
        if (GUIMain.GUICraft.ActiveCraftInfo == _craftInfo)
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
