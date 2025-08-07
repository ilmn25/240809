using UnityEngine;

class MobChase : State {
    public override void OnEnterState()
    {
        Module<PathingModule>().SetTarget(PathingTarget.Target);
    }
    
    public override void OnUpdateState() {
        if (Module<MobStatusModule>().PathingStatus != PathingStatus.Pending) Machine.SetState<DefaultState>();
    }
}
 