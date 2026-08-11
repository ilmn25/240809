using UnityEngine;

/// <summary>Stalker follow: creeps toward the target but stops at a set distance,
/// so stalker enemies trail the player from afar instead of closing to melee.</summary>
public class MobStalk : MobState
{
    private readonly float _stopDistance;

    public MobStalk(float stopDistance)
    {
        _stopDistance = stopDistance;
    }

    public override void OnEnterState()
    {
        Module<PathingModule>().SetTarget(PathingTarget.Target);
    }

    public override void OnUpdateState()
    {
        if (Info.PathingStatus != PathingStatus.Pending)
        {
            Machine.SetState<DefaultState>();
            return;
        }
        if (Info.Target == null ||
            Vector3.Distance(Machine.transform.position, Info.Target.position) < _stopDistance)
            Info.PathingStatus = PathingStatus.Reached;
    }
}
