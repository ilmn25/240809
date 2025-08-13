class MobChaseInteract : MobState {
    public override void OnEnterState()
    {
        Module<PathingModule>().SetTarget(PathingTarget.Target);
    }
    
    public override void OnUpdateState() {
        if (Info.PathingStatus == PathingStatus.Reached)
        {
            if (Info.Target && Info.ActionTarget == IActionTarget.Secondary)
                ((IActionSecondary)Info.Action).OnActionSecondary((EntityMachine) Machine); 
            Info.Target = null;
            Machine.SetState<DefaultState>();
        }
        else if (Info.PathingStatus == PathingStatus.Stuck)
        {
            Info.Target = null;
            Machine.SetState<DefaultState>();
        }
    } 
}