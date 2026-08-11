using UnityEngine;

/// <summary>Keeps fleeing from the target until it gets far enough away, then
/// despawns. Used by stalker enemies that vanish after being discovered or when
/// day breaks.</summary>
public class MobFleeDespawn : MobState
{
    private readonly float _fleeDistance;

    public MobFleeDespawn(float fleeDistance)
    {
        _fleeDistance = fleeDistance;
    }

    public override void OnEnterState()
    {
        Info.FaceTarget = false;
        Module<PathingModule>().SetTarget(PathingTarget.Escape);
    }

    public override void OnUpdateState()
    {
        if (Info.Target == null)
        {
            Info.Destroy();
            return;
        }

        if (Info.PathingStatus != PathingStatus.Pending)
            Module<PathingModule>().SetTarget(PathingTarget.Escape);

        if (Vector3.Distance(Machine.transform.position, Info.Target.position) > _fleeDistance)
            Info.Destroy();
    }
}
