using UnityEngine;

class MobRoam : State {
    private PathingModule _pathingModule;
    private MobStatusModule _mobStatusModule;
    
    public override void OnInitialize()
    {
        _pathingModule = Machine.GetModule<PathingModule>();
        _mobStatusModule = Machine.GetModule<MobStatusModule>();
    }

    public override void OnEnterState()
    {
        _pathingModule.SetTarget(null);
    }
    
    public override void OnUpdateState() {
        if (_mobStatusModule.PathingStatus != PathingStatus.Pathing) Parent.SetState<StateEmpty>();
    }
}