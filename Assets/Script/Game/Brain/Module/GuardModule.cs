using UnityEngine;

/// <summary>The one thing that sets an outpost guard apart from a regular mob:
/// it deaggros and returns home once dragged too far from the camp that spawned
/// it. Aggro itself is handled by the base machine — engage intruders near the
/// guard itself.</summary>
public class GuardModule : MobModule
{
    /// <summary>World position of the outpost this guard protects.</summary>
    public Vector3 HomePosition;

    /// <summary>How far from home the guard may be dragged before breaking off.</summary>
    private const float LeashRadius = 10f;

    /// <summary>True when the guard has been dragged beyond its leash and should
    /// break off its attack and return to the outpost.</summary>
    public bool IsBeyondLeash =>
        Vector3.Distance(Machine.transform.position, HomePosition) > LeashRadius;

    public override void Update()
    {
        if (!Helper.IsHost()) return;

        // Only kick off the return-home once. Re-entering MobReturnHome every
        // frame calls SetTarget(Home), which clears the path and re-runs async
        // pathfinding — the home route never completes and the guard freezes.
        if (IsBeyondLeash && !Machine.IsCurrentState<MobReturnHome>())
        {
            Info.CancelTarget();
            Machine.SetState<MobReturnHome>();
        }
    }
}
