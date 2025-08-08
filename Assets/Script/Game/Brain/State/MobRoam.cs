class MobRoam : MobState {
    public override void OnEnterState()
    {
        Module<PathingModule>().SetTarget(PathingTarget.Roam);
    }
    
    public override void OnUpdateState() {
        if (Info.PathingStatus != PathingStatus.Pending) Machine.SetState<DefaultState>();
    }
}

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
