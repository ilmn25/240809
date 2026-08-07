using UnityEngine;

class MobEscape : MobState {
    public override void OnEnterState()
    {
        // Face the direction of movement (away from the threat), not the target.
        Info.FaceTarget = false;
        Module<PathingModule>().SetTarget(PathingTarget.Escape);
    }
    
    public override void OnUpdateState() {
        if (Info.PathingStatus != PathingStatus.Pending)
        {
            Machine.SetState<DefaultState>();
            return;
        }
        if (Info.Target == null || Vector3.Distance(Machine.transform.position, Info.Target.position) > Info.DistAttack + 1)
            Info.PathingStatus = PathingStatus.Reached;  
    }
}