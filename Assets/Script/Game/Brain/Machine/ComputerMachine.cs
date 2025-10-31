public class ComputerMachine : StructureMachine, IActionSecondaryInteract
{
    public static Info CreateInfo()
    {
        return new Info();
    }

    public override void OnStart()
    {
        base.OnStart();
        AddState(new InComputerState()); 
    }

    public void OnActionSecondary(Info info)
    { 
        if (IsCurrentState<DefaultState>())
            SetState<InComputerState>();
        else
        {
            SetState<DefaultState>();
        }  
    }
}

public class InComputerState : State
{
    public override void OnEnterState()
    {
        Audio.PlaySFX(SfxID.Text);
        GUIMain.GUIMenu.Show(true);
    }

    public override void OnUpdateState()
    {
        if (Helper.SquaredDistance(Main.Player.transform.position, Machine.transform.position) > 36) { 
            Machine.SetState<DefaultState>();
        }
    }

    public override void OnExitState()
    {
        GUIMain.GUIMenu.Show(false);
    }
}