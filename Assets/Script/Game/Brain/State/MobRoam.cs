class MobRoam : State {
    private PathingModule _pathingModule;
    private MobStatusModule _mobStatusModule;
    
    public override void Initialize()
    {
        _pathingModule = Machine.GetModule<PathingModule>();
        _mobStatusModule = Machine.GetModule<MobStatusModule>();
    }

    public override void OnEnterState()
    {
        _pathingModule.SetTarget(PathingTarget.Roam);
    }
    
    public override void OnUpdateState() {
        if (_mobStatusModule.PathingStatus != PathingStatus.Pending) Machine.SetState<DefaultState>();
    }
}

class MobEvade : State {
    private PathingModule _pathingModule;
    private MobStatusModule _mobStatusModule;
    
    public override void Initialize()
    {
        _pathingModule = Machine.GetModule<PathingModule>();
        _mobStatusModule = Machine.GetModule<MobStatusModule>();
    }

    public override void OnEnterState()
    {
        _pathingModule.SetTarget(PathingTarget.Evade);
    }
    
    public override void OnUpdateState()
    {
        _mobStatusModule.Direction.y = 0.5f;
        if (_mobStatusModule.PathingStatus != PathingStatus.Pending) Machine.SetState<DefaultState>();
    }
}
 