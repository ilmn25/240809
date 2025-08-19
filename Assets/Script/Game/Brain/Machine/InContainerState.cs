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
        if (!GUIMain.Showing || Helper.SquaredDistance(Game.Player.transform.position, Machine.transform.position) > 5*5) { //walk away from npc
            Machine.SetState<DefaultState>();
        }
    }

    public override void OnExitState()
    {
        GUIMain.Storage.Show(false);
    }
}