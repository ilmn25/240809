using UnityEngine;

/// <summary>Keeps a flying mob airborne at a fixed height and flies it in toward
/// the target anchor (defaults to the player), then hovers once overhead. Runs in
/// LateUpdate (after ground movement) so gravity can't pull it down. Altitude is
/// maintained even while fleeing so it never touches the floor.</summary>
public class HoverFlightModule : MobModule
{
    public HoverFlightModule() { updateMode = UpdateMode.Everyone; }

    public float HoverHeight = 10f;
    public float ApproachSpeed = 8f;
    public float ApproachDistance = 1.5f;

    /// <summary>True once the mob has flown in and is hovering over the anchor.</summary>
    public bool IsOverhead { get; private set; }

    public override void Update()
    {
        if (!Helper.IsHost() && !Info.IsOwner()) return;

        Info target = Info.Target;
        Vector3 anchor = (target != null && !target.Destroyed)
            ? target.position
            : (Main.PlayerInfo != null && !Main.PlayerInfo.Destroyed ? Main.PlayerInfo.position : Machine.transform.position);

        Vector3 pos = Machine.transform.position;
        float desiredY = anchor.y + HoverHeight;

        // Fleeing — let flee logic handle horizontal movement; only hold altitude.
        if (Machine.IsCurrentState<MobFleeDespawn>())
        {
            pos.y = Mathf.MoveTowards(pos.y, desiredY, 20f * Time.deltaTime);
            Machine.transform.position = pos;
            return;
        }

        // Fly in horizontally toward the anchor, then hover once overhead.
        Vector3 here = new Vector3(pos.x, 0, pos.z);
        Vector3 there = new Vector3(anchor.x, 0, anchor.z);
        float flatDist = Vector3.Distance(here, there);
        IsOverhead = flatDist <= ApproachDistance;
        if (!IsOverhead)
        {
            Vector3 dir = (there - here).normalized;
            pos.x += dir.x * ApproachSpeed * Time.deltaTime;
            pos.z += dir.z * ApproachSpeed * Time.deltaTime;
        }

        pos.y = Mathf.MoveTowards(pos.y, desiredY, 20f * Time.deltaTime);
        Machine.transform.position = pos;
        Info.Direction = Vector3.zero;
    }
}
