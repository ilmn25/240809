using UnityEngine;

/// <summary>Keeps a flying mob airborne around a fixed height above a target anchor
/// (defaults to the player) and flies it in toward it, then bobs in place once
/// overhead. Altitude is held the way a flapping bird does it — a wing-flap impulse
/// kicks velocity up, gravity pulls it back down in an arc, and it flaps again as it
/// falls below the band, giving the bouncing flight harpies get from jumping. When it
/// starts out high above the band (a sky spawn) it brakes its fall into a steady glide
/// down instead of plummeting.</summary>
public class HoverFlightModule : MobModule
{
    public HoverFlightModule() { updateMode = UpdateMode.Everyone; }

    public float HoverHeight = 10f;
    public float ApproachDistance = 2.5f;
    public float FlapVelocity = 10f; // upward kick of one wing flap
    public float FlapDrop = 0.6f;    // how far below the hover height it falls before flapping
    public float MaxDescent = 8f;    // fastest it sinks while gliding down from above

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

        // Fleeing (or leaving) — flee drives horizontal; just flap so it stays airborne
        // at hover height while it flies away.
        if (Machine.IsCurrentState<MobFleeDespawn>() || (Machine is PigeonMachine p && p.Leaving))
        {
            Fly(pos.y, desiredY);
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

        Info.Direction = dir;
        Fly(pos.y, desiredY);
    }

    /// <summary>Vertical control: glide down steadily while high above the hover band
    /// (a sky spawn dropping in), otherwise flap on the way down to bounce along it.</summary>
    private void Fly(float y, float desiredY)
    {
        // High above the band — brake the fall so it swoops down instead of plummeting.
        if (y > desiredY + FlapDrop)
        {
            Info.Velocity.y = Mathf.Max(Info.Velocity.y, -MaxDescent);
            return;
        }

        // Only flap on the way down — flapping while still rising would cancel the arc
        // and turn the bounce into a flat glide.
        if (Info.Velocity.y > 0f) return;
        if (y > desiredY - FlapDrop) return;

        Info.Velocity.y = FlapVelocity;
        Info.IsGrounded = false;
    }
}
