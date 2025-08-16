class MobStrafe : MobState {
    public override void OnEnterState()
    { 
        Module<PathingModule>().SetTarget(PathingTarget.Strafe);
    }
    
    public override void OnUpdateState()
    {
        if (Info.PathingStatus != PathingStatus.Pending) Machine.SetState<DefaultState>();
    }
}