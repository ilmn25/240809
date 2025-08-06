using UnityEngine;

class MobChase : State {
    private GroundMovementModule _groundMovementModule;
    private PathingModule _pathingModule;
    private MobStatusModule _mobStatusModule;
    
    public override void OnInitialize()
    {
        _groundMovementModule = Machine.GetModule<GroundMovementModule>();
        _pathingModule = Machine.GetModule<PathingModule>();
        _mobStatusModule = Machine.GetModule<MobStatusModule>();
    }

    public override void OnEnterState()
    {
        _pathingModule.SetTarget(_mobStatusModule.Target);
    }
    
    public override void OnUpdateState() {
        if (_mobStatusModule.PathingStatus != PathingStatus.Pathing) Parent.SetState<StateEmpty>();
    }
}