using UnityEngine;

/// <summary>Keeps fleeing from the target until it gets far enough away, then
/// despawns. Ground mobs flee along a path away from the target; flying mobs
/// (CanFly) just fly straight away through the air (no ground path needed).
/// Used by stalker enemies that vanish after being discovered or when day breaks.</summary>
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
        // Flying mobs flee straight through the air — no ground path required.
        if (Info.CanFly)
            return;
        Module<PathingModule>().SetTarget(PathingTarget.Escape);
    }

    public override void OnUpdateState()
    {
        if (Info.Target == null)
        {
            Info.Destroy();
            return;
        }

        if (Info.CanFly)
        {
            // Steer directly away from the target (airborne — no ground path needed).
            // Altitude is HoverFlightModule's job, so only touch the horizontal and
            // leave Direction.y alone.
            Vector3 away = Machine.transform.position - Info.Target.position;
            away.y = 0;
            Vector3 dir = away.sqrMagnitude > 0.001f ? away.normalized : Vector3.right;
            Info.Direction.x = dir.x;
            Info.Direction.z = dir.z;
        }
        else if (Info.PathingStatus != PathingStatus.Pending)
        {
            Module<PathingModule>().SetTarget(PathingTarget.Escape);
        }

        if (Vector3.Distance(Machine.transform.position, Info.Target.position) > _fleeDistance)
            Info.Destroy();
    }
}
