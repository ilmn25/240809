using UnityEngine;

/// <summary>Shared guard behavior for outpost defenders (raider guards, scout
/// guards), modeled after the Clockwork Knight in DST: the guard stays near its
/// home (the dirty tent) and only engages intruders who come close to the camp.
/// It never chases far — if the target leaves the camp's vicinity, the guard
/// breaks off and returns to its post.
///
/// Add this module to any GroundMobMachine and set <see cref="HomePosition"/> to
/// make it a territorial guard.</summary>
public class GuardModule : MobModule
{
    /// <summary>World position of the outpost this guard protects.</summary>
    public Vector3 HomePosition;

    /// <summary>How close an intruder must get to the camp to be engaged.</summary>
    private const float AlertRadius = 8f;

    /// <summary>How far from home the guard may chase before breaking off.</summary>
    private const float LeashRadius = 10f;

    public override void Update()
    {
        if (!Helper.IsHost()) return;

        // If we've been dragged too far from home, break off and head back.
        if (Vector3.Distance(Machine.transform.position, HomePosition) > LeashRadius)
        {
            Info.CancelTarget();
            Machine.SetState<MobReturnHome>();
            return;
        }

        // Only engage intruders who come close to the camp. If the current target
        // leaves the camp's vicinity, break off and return to the post.
        Info intruder = FindNearestIntruder();
        if (intruder == null)
        {
            if (Info.Target != null)
            {
                Info.CancelTarget();
                Machine.SetState<MobReturnHome>();
            }
            return;
        }

        // Lock onto the intruder so the guard attacks them (via the base machine).
        if (Info.Target != intruder)
        {
            Info.Target = intruder;
            Info.PathingStatus = PathingStatus.Pending;
        }
    }

    /// <summary>Nearest player or friendly NPC within alert range of the camp.</summary>
    private Info FindNearestIntruder()
    {
        Info best = (Main.PlayerInfo != null && !Main.PlayerInfo.Destroyed &&
                     Vector3.Distance(Main.PlayerInfo.position, HomePosition) <= AlertRadius)
            ? Main.PlayerInfo : null;
        Info npc = EntityScan.FindNearest(HomePosition, AlertRadius, i => i is DynamicInfo d && d.IsNPC);
        if (npc != null && (best == null ||
            (npc.position - HomePosition).sqrMagnitude < (best.position - HomePosition).sqrMagnitude))
            best = npc;
        return best;
    }
}
