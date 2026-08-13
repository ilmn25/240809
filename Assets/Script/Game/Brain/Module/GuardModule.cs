using UnityEngine;

/// <summary>Shared guard behavior for outpost defenders (raider guards, scout
/// guards). Keeps the mob near its home (the dirty tent), aggroes on anyone who
/// gets too close to the tent, and breaks off pursuit to return home once the
/// target escapes the leash or the guard is dragged too far from home.
///
/// Add this module to any GroundMobMachine and set <see cref="HomePosition"/> to
/// make it a territorial guard.</summary>
public class GuardModule : MobModule
{
    /// <summary>World position of the outpost this guard protects.</summary>
    public Vector3 HomePosition;

    private const float LeashRadius = 14f;    // how far it will chase from home
    private const float TentAlertRadius = 8f; // how close an intruder must get to the tent to trigger the guard

    public override void Update()
    {
        if (!Helper.IsHost()) return;

        // If we've been dragged too far from home, head back.
        if (Vector3.Distance(Machine.transform.position, HomePosition) > LeashRadius)
        {
            Info.CancelTarget();
            Machine.SetState<MobReturnHome>();
            return;
        }

        // Defend the outpost: only chase while the target stays within the leash.
        // If it flees past the leash, break off and return home.
        if (Info.Target != null &&
            Vector3.Distance(Info.Target.position, HomePosition) > LeashRadius)
        {
            Info.CancelTarget();
            Machine.SetState<MobReturnHome>();
            return;
        }

        // Aggro on anyone who gets too close to the tent itself.
        if (Info.Target == null)
        {
            Info nearest = FindNearestIntruder();
            if (nearest != null)
            {
                Info.Target = nearest;
                Info.PathingStatus = PathingStatus.Pending;
                // Snap back to DefaultState so the mob's own OnUpdate (which only
                // acts in DefaultState) picks up the new target and starts chasing.
                Machine.SetState<DefaultState>();
            }
        }
    }

    /// <summary>Nearest player or friendly NPC within alert range of the tent.</summary>
    private Info FindNearestIntruder()
    {
        Info best = (Main.PlayerInfo != null && !Main.PlayerInfo.Destroyed &&
                     Vector3.Distance(Main.PlayerInfo.position, HomePosition) <= TentAlertRadius)
            ? Main.PlayerInfo : null;
        Info npc = EntityScan.FindNearest(HomePosition, TentAlertRadius, i => i is DynamicInfo d && d.IsNPC);
        if (npc != null && (best == null ||
            (npc.position - HomePosition).sqrMagnitude < (best.position - HomePosition).sqrMagnitude))
            best = npc;
        return best;
    }
}
