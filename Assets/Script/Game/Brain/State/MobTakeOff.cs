using UnityEngine;

/// <summary>Flying escape for ground birds: climbs while heading directly away from
/// the threat, then despawns once it is high and far enough away (or after a short
/// while, so it is never left circling forever). The threat is captured on entry and
/// the target cleared, so the bird faces its flight path instead of the pursuer.</summary>
public class MobTakeOff : MobState
{
    private const int MaxFlightFrames = 60 * 6;

    private readonly float _flightHeight;
    private readonly float _fleeDistance;

    private Vector3 _threat;
    private float _takeoffY;
    private int _flightTimer;

    public MobTakeOff(float flightHeight, float fleeDistance)
    {
        _flightHeight = flightHeight;
        _fleeDistance = fleeDistance;
    }

    public override void OnEnterState()
    {
        Info.FaceTarget = false;
        Info.CanFly = true;
        _threat = Info.Target != null ? Info.Target.position : Machine.transform.position + Vector3.right;
        _takeoffY = Machine.transform.position.y;
        _flightTimer = 0;

        // Clear the target and any ground path so pathing can't steer the bird
        // while it is flying away.
        Info.Target = null;
        PathingModule pathing = Module<PathingModule>();
        if (pathing != null)
        {
            pathing.PathingTarget = PathingTarget.None;
            Info.PathingStatus = PathingStatus.Reached;
        }
    }

    public override void OnUpdateState()
    {
        Vector3 position = Machine.transform.position;

        Vector3 away = position - _threat;
        away.y = 0f;
        Vector3 direction = away.sqrMagnitude > 0.001f ? away.normalized : Vector3.right;
        // Beat its wings until it clears the treetops, then glide out.
        direction.y = position.y - _takeoffY < _flightHeight ? 1f : 0f;
        Info.Direction = direction;

        _flightTimer++;
        Vector3 flat = position - _threat;
        flat.y = 0f;
        if (_flightTimer >= MaxFlightFrames || flat.magnitude > _fleeDistance)
            Info.Destroy();
    }
}
