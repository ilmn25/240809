using UnityEngine;

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
        PlayerTerraform.ToolData = Item.GetItem(Inventory.CurrentItem.StringID);
    }

    public override void OnExitState()
    {
        PlayerTerraform.ToolData = null;
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