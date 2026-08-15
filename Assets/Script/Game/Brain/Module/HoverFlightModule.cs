using UnityEngine;

/// <summary>Keeps a flying mob airborne at a fixed height above a target anchor
/// (defaults to the player) and flies it in toward it, then hovers once overhead.
/// Altitude is maintained with the same jump logic the pathfinder uses: when the
/// mob is below the hover height it sets Direction.y > 0 so GroundMovement gives
/// it a jump — keeping it airborne without pinning or zeroing its velocity.</summary>
public class HoverFlightModule : MobModule
{
    public HoverFlightModule() { updateMode = UpdateMode.Everyone; }

    public float HoverHeight = 10f;
    public float ApproachDistance = 2.5f;

    private const float HoverTolerance = 0.5f;

    /// <summary>True once the mob has flown in and is hovering over the anchor.</summary>
    public bool IsOverhead { get; private set; }

    public override void Update()
    {
        if (!Helper.IsHost() && !Info.IsOwner()) return;

        Info target = Info.Target;
        Vector3 anchor = (target != null && !target.Destroyed)
            ? target.position
            : (Main.PlayerInfo != null && !Main.PlayerInfo.Destroyed ? Main.PlayerInfo.position : Machine.transform.position);

        float desiredY = anchor.y + HoverHeight;
        Vector3 pos = Machine.transform.position;

        // Fleeing (or leaving) — flee drives horizontal; just keep it airborne so it
        // stays at hover height while it flies away.
        if (Machine.IsCurrentState<MobFleeDespawn>() || (Machine is PigeonMachine p && p.Leaving))
        {
            Info.Direction.y = pos.y < desiredY - HoverTolerance ? 1f : 0f;
            return;
        }

        // Fly in toward the anchor horizontally, then hover once overhead.
        Vector3 here = new Vector3(pos.x, 0, pos.z);
        Vector3 there = new Vector3(anchor.x, 0, anchor.z);
        float flatDist = Vector3.Distance(here, there);
        IsOverhead = flatDist <= ApproachDistance;

        Vector3 dir = Vector3.zero;
        if (!IsOverhead)
            dir = new Vector3(there.x - here.x, 0, there.z - here.z).normalized;

        // Jump to hover height when below it, exactly like pathfinding triggers a
        // jump (Direction.y > 0 → GroundMovement.HandleJump sets JumpVelocity).
        if (pos.y < desiredY - HoverTolerance)
            dir.y = 1f;

        Info.Direction = dir;
    }
}
