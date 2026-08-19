using UnityEngine;

class MobEscape : MobState {
    public override void OnEnterState()
    {
        // Face the direction of movement (away from the threat), not the target.
        Info.FaceTarget = false;
        Module<PathingModule>().SetTarget(PathingTarget.Escape);
    }
    
    public override void OnUpdateState()
    {
        TryEndEscape();
    }

    /// <summary>Shared escape exit: stops fleeing when the path is done or the
    /// threat has backed off. Returns true when the escape ended this frame.</summary>
    protected bool TryEndEscape()
    {
        if (Info.PathingStatus != PathingStatus.Pending)
        {
            Machine.SetState<DefaultState>();
            return true;
        }
        if (Info.Target == null || Vector3.Distance(Machine.transform.position, Info.Target.position) > Info.DistAttack + 1)
        {
            Info.PathingStatus = PathingStatus.Reached;
            return true;
        }
        return false;
    }
}