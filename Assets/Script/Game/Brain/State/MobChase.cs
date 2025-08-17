using UnityEditor;
using UnityEngine;

class MobChase : MobState {
    public override void OnEnterState()
    {
        Module<PathingModule>().SetTarget(PathingTarget.Target);
    }
    
    public override void OnUpdateState() {
        if (Info.PathingStatus != PathingStatus.Pending)
        {
            Machine.SetState<DefaultState>();
            return;
        }
        if (Info.Target == null || Vector3.Distance(Machine.transform.position, Info.Target.position) < Info.DistAttack)
            Info.PathingStatus = PathingStatus.Reached;  
    }
}