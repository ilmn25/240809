class MobRoam : MobState {
    public override void OnEnterState()
    {
        Module<PathingModule>().SetTarget(PathingTarget.Roam);
    }
    
    public override void OnUpdateState() {
        if (Info.PathingStatus != PathingStatus.Pending) Machine.SetState<DefaultState>();
    }
} 