using UnityEngine;

/// <summary>Homing flight for eye-like enemies. The eye hovers toward its target
/// along a straight line like a guided missile — no ground pathing or gravity —
/// holding a set altitude above it. With no target it bleeds off speed and hovers
/// in place.</summary>
public class HoverMovementModule : MovementModule
{
    public float HoverHeight = 3f;    // altitude above the target's position
    public float StopDistance = 1.2f; // how close it flies before holding position
    public float TurnSpeed = 3f;      // how quickly it re-aims at the target

    private const float RoamRadius = 5f;      // how far it drifts while idle
    private const float RoamMinInterval = 2f; // seconds between idle course changes
    private const float RoamMaxInterval = 4f;

    private Vector3 _roamPoint;
    private float _roamTimer;

    public HoverMovementModule(float hoverHeight = 3f, float stopDistance = 1.2f, float turnSpeed = 3f)
    {
        updateMode = UpdateMode.Everyone;
        HoverHeight = hoverHeight;
        StopDistance = stopDistance;
        TurnSpeed = turnSpeed;
    }

    public override void Update()
    {
        if (!Helper.IsHost() && !Info.IsOwner()) return;
        if (Info.Health <= 0) return;

        DeltaTime = Helper.GetDeltaTime();
        Vector3 pos = Machine.transform.position;
        Vector3 desired = Anchor() + Vector3.up * HoverHeight;
        Vector3 toTarget = desired - pos;
        float distance = toTarget.magnitude;

        float speed = Info.SpeedAir * Info.SpeedModifier;
        if (speed <= 0f || distance <= StopDistance)
        {
            // Stopped (e.g., mid-swing) or overhead — bleed off speed and hover.
            Info.Velocity = Vector3.MoveTowards(Info.Velocity, Vector3.zero, TurnSpeed * Info.SpeedAir * DeltaTime);
        }
        else
        {
            // Steer the current velocity toward the target like a missile.
            Vector3 desiredVelocity = (toTarget / distance) * speed;
            Info.Velocity = Vector3.MoveTowards(Info.Velocity, desiredVelocity, TurnSpeed * speed * DeltaTime);
        }

        Machine.transform.position = pos + Info.Velocity * DeltaTime;
        Info.position = Machine.transform.position;

        // Drive the sprite's facing from actual flight direction so flying
        // enemies flip to face where they're moving (or hover in place when
        // stopped), matching ground mobs.
        Vector3 facing = Info.Velocity;
        facing.y = 0f;
        Info.Direction = facing.sqrMagnitude > 0.0001f ? facing.normalized : Vector3.zero;
    }

    private Vector3 Anchor()
    {
        MobInfo mob = Info as MobInfo;
        Info target = mob?.Target;
        if (target != null && !target.Destroyed) return target.position;
        return RoamPoint();
    }

    // No target — drift toward a fresh random point nearby every few seconds so
    // idle fliers circle their spawn instead of freezing in mid-air.
    private Vector3 RoamPoint()
    {
        _roamTimer -= DeltaTime;
        if (_roamTimer <= 0f)
        {
            _roamTimer = Random.Range(RoamMinInterval, RoamMaxInterval);
            Vector3 pos = Machine.transform.position;
            _roamPoint = pos + new Vector3(
                Random.Range(-RoamRadius, RoamRadius),
                Random.Range(-RoamRadius * 0.5f, RoamRadius * 0.5f),
                Random.Range(-RoamRadius, RoamRadius));
        }
        return _roamPoint;
    }
}
