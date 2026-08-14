using UnityEngine;

/// <summary>Holds a flying mob at a fixed height above a target (defaults to the
/// player). Runs in LateUpdate (after ground movement) so gravity can't pull it
/// down. The mob's own OnUpdate decides when to start fleeing/leaving.</summary>
public class HoverFlightModule : MobModule
{
    public HoverFlightModule() { updateMode = UpdateMode.Everyone; }

    public float HoverHeight = 10f;

    public override void Update()
    {
        if (!Helper.IsHost() && !Info.IsOwner()) return;

        // While fleeing, let the mob fly away freely (don't hold it in place).
        if (Machine.IsCurrentState<MobFleeDespawn>()) return;

        Info target = Info.Target;
        Vector3 anchor = (target != null && !target.Destroyed)
            ? target.position
            : (Main.PlayerInfo != null && !Main.PlayerInfo.Destroyed ? Main.PlayerInfo.position : Machine.transform.position);

        Vector3 pos = Machine.transform.position;
        // Keep horizontal position over the anchor, hold at hover height.
        pos.x = anchor.x;
        pos.z = anchor.z;
        pos.y = Mathf.MoveTowards(pos.y, anchor.y + HoverHeight, 20f * Time.deltaTime);
        Machine.transform.position = pos;
        Info.Direction = Vector3.zero;
    }
}
