 

public class ItemToolState : State
{ 
    ToolAnimationModule _toolAnimationModule;
    public override void OnInitialize()
    {
        _toolAnimationModule = GetModule<ToolAnimationModule>();
    }

    public override void OnEnterState()
    {
        _toolAnimationModule.ShowTool(true);
    }

    public override void OnExitState()
    {
        _toolAnimationModule.ShowTool(false);
    }

    public override void OnUpdateState()
    { 
        if (!_toolAnimationModule.IsBusy())
        {
            if (Control.Inst.ActionPrimary.KeyDown() || 
                Control.Inst.DigUp.KeyDown() ||
                Control.Inst.DigDown.KeyDown())
            {
                _toolAnimationModule.StartAnimation();
            }
        } 
        _toolAnimationModule.HandleAnimationUpdate();
    }
}