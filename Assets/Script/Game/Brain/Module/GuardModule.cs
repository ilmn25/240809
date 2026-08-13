using UnityEngine;

/// <summary>Shared guard behavior for outpost defenders (raider guards, scout
/// guards). Simply keeps the mob near its home (the dirty tent): if it wanders
/// more than <see cref="StayRadius"/> blocks from home, it breaks off and returns.
/// Otherwise it behaves like its base mob (aggro on sight, chase, attack).
///
/// Add this module to any GroundMobMachine and set <see cref="HomePosition"/> to
/// make it a territorial guard.</summary>
public class GuardModule : MobModule
{
    /// <summary>World position of the outpost this guard protects.</summary>
    public Vector3 HomePosition;

    /// <summary>How far from home the guard may roam before returning.</summary>
    private const float StayRadius = 5f;

    public override void Update()
    {
        if (!Helper.IsHost()) return;

        // If we've wandered too far from home, break off and head back.
        if (Vector3.Distance(Machine.transform.position, HomePosition) > StayRadius)
        {
            Info.CancelTarget();
            Machine.SetState<MobReturnHome>();
        }
    }
}
