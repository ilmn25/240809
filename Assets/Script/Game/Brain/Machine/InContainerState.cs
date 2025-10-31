public class InContainerState : State
{
    public Storage Storage;
    public override void OnEnterState()
    {
        Audio.PlaySFX(SfxID.Text);
        GUIMain.Storage.Storage = Storage;
        GUIMain.RefreshStorage(); 
        GUIMain.Show(true);
        GUIMain.Storage.Show(true, !GUIMain.Showing);
    }

    public override void OnUpdateState()
    {
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