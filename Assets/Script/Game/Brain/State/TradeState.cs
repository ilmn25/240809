/// <summary>Opens a trader's window through the craft UI — the merchant's and the bandwagon
/// leader's stock, the money changer's coin trades, the collector's scrap. Mirrors
/// InCraftState, but the menu and what clicking it does come from the machine (see ITrader)
/// instead of the entity's own Info.</summary>
public class TradeState : State
{
    public override bool IsInteraction => true;

    private CraftInfo _menu;

    public override void OnEnterState()
    {
        Audio.PlaySFX(SfxID.Text);

        ITrader trader = (ITrader)Machine;
        _menu = trader.Menu;

        GUIMain.GUICraft.FitSlots(_menu.GetStoragePool());
        GUIMain.GUICraft.Use(_menu, trader.Mode);

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

        if (GUIMain.GUICraft.ActiveCraftInfo != _menu)
            Machine.SetState<DefaultState>();
    }

    public override void OnExitState()
    {
        if (GUIMain.GUICraft.ActiveCraftInfo == _menu)
        {
            GUIMain.GUICraft.UseDefaultStorage();
            GUIMain.RefreshStorage();
        }

        _menu = null;
    }
}
