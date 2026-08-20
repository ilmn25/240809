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

        if (distance > StopDistance)
        {
            // Steer the current velocity toward the target like a missile.
            Vector3 desiredVelocity = (toTarget / distance) * Info.SpeedAir;
            Info.Velocity = Vector3.MoveTowards(Info.Velocity, desiredVelocity, TurnSpeed * Info.SpeedAir * DeltaTime);
        }
        else
        {
            // Overhead — bleed off speed and hover in place.
            Info.Velocity = Vector3.MoveTowards(Info.Velocity, Vector3.zero, TurnSpeed * Info.SpeedAir * DeltaTime);
        }

        Machine.transform.position = pos + Info.Velocity * DeltaTime;
        Info.position = Machine.transform.position;
    }

    private Vector3 Anchor()
    {
        MobInfo mob = Info as MobInfo;
        Info target = mob?.Target;
        if (target != null && !target.Destroyed) return target.position;
        return Machine.transform.position; // no target — hover in place
    }
}
