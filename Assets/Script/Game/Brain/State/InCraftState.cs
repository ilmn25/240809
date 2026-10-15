public class InCraftState : State
{
    private CraftInfo _craftInfo;

    public override void OnEnterState()
    {
        Audio.PlaySFX(SfxID.Text);

        _craftInfo = (CraftInfo)Info;
        GUIMain.GUICraft.FitSlots(_craftInfo.GetStoragePool());

        GUIMain.GUICraft.Use(_craftInfo, TradeMode.Craft);

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
}
