public class InContainerState : State
{
    public Storage Storage;
    private int _playerIndex = -1;
    public override void OnEnterState()
    {
        Audio.PlaySFX(SfxID.Text);
        _playerIndex = Control.CurrentPlayerIndex;
        GUIMain.Storage.Storage = Storage;
        GUIMain.RefreshStorage(); 
        GUIMain.Show(true);
        GUIMain.Storage.Show(Storage != Main.PlayerInfo.Storage, !GUIMain.Showing);
    }

    public override void OnUpdateState()
    {
        if (_playerIndex != Control.CurrentPlayerIndex)
        {
            _playerIndex = Control.CurrentPlayerIndex;
            if (Storage == Main.PlayerInfo.Storage)
                GUIMain.Storage.Show(false);
        }

        if (Main.Player == null)
        {
            Machine.SetState<DefaultState>();
            return;
        }

        if (!GUIMain.Showing || GUIMain.Storage.Storage != Storage ||
            Helper.SquaredDistance(Main.Player.transform.position, Machine.transform.position) > 5*5) { //walk away from npc
            Machine.SetState<DefaultState>();
        }
    }

    public override void OnExitState()
    {
        if (GUIMain.Storage.Storage == Storage)
            GUIMain.Storage.Show(false);
    }
}