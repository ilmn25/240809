class MobChaseAction : MobState {
    public override void OnEnterState()
    {
        Module<PathingModule>().SetTarget(PathingTarget.Target);
    }
    
    public override void OnUpdateState() {
        if (Info.PathingStatus == PathingStatus.Reached)
        {
            if (Info.Target)
            {
                if (Info.ActionTarget == IActionTarget.Secondary)
                {
                    ((IActionSecondary)Info.Action).OnActionSecondary((EntityMachine) Machine);
                    Info.Target = null;
                    Machine.SetState<DefaultState>();
                } 
                else if (Info.ActionTarget == IActionTarget.Hit && Info.Equipment is { Type: ItemType.Tool }
                         && Info.Target.gameObject.activeSelf)
                {
                    ((EntityMachine)Machine).Attack();
                }
                else
                {
                    Info.Target = null;
                    Machine.SetState<DefaultState>();
                }
            }  
        }
        else if (Info.PathingStatus == PathingStatus.Stuck)
        {
            Info.Target = null;
            Machine.SetState<DefaultState>();
        }
    } 
}