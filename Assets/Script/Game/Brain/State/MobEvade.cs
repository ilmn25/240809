class MobEvade : MobState {
    public override void OnEnterState()
    {
        Module<PathingModule>().SetTarget(PathingTarget.Evade);
    }
    
    public override void OnUpdateState()
    {
        Info.Direction.y = 0.5f;
        if (Info.PathingStatus != PathingStatus.Pending) Machine.SetState<DefaultState>();
    }
}