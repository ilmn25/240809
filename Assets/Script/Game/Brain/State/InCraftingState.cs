public class InCraftingState : State
{
    public override void OnEnterState()
    {
        Audio.PlaySFX(SfxID.Text);
        GUIMain.Crafting.Storage = ((ContainerInfo)Info).Storage;
        GUIMain.RefreshStorage(); 
        GUIMain.Show(true);
        GUIMain.Crafting.Show(true, !GUIMain.Showing);
    }

    public override void OnUpdateState()
    {
        if (!GUIMain.Showing || Helper.SquaredDistance(Game.Player.transform.position, Machine.transform.position) > 36) { //walk away from npc
            Machine.SetState<DefaultState>();
        }
    }

    public override void OnExitState()
    {
        GUIMain.Crafting.Show(false);
    }
}

public class InConverterState : State
{
    public override void OnEnterState()
    {
        Audio.PlaySFX(SfxID.Text);
        GUIMain.Converter.Storage = ((ConverterInfo)Info).Storage;
        GUIMain.Converter.Info = ((ConverterInfo)Info);
        GUIMain.RefreshStorage(); 
        GUIMain.Show(true);
        GUIMain.Converter.Show(true, !GUIMain.Showing);
    }

    public override void OnUpdateState()
    {
        if (!GUIMain.Showing || Helper.SquaredDistance(Game.Player.transform.position, Machine.transform.position) > 36) { //walk away from npc
            Machine.SetState<DefaultState>();
        }
    }

    public override void OnExitState()
    {
        GUIMain.Converter.Show(false);
    }
}