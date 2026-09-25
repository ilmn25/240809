using UnityEngine;

public class MobMachine : EntityMachine, IActionPrimaryAttack
{
    public new MobInfo Info => GetModule<MobInfo>();
    public override void OnSetup()
    {
        if (Info.CharSprite == ID.Null) Info.CharSprite = Info.id;
        transform.Find("Sprite").Find("Char").GetComponent<SpriteRenderer>().sprite =
            Cache.LoadSprite("Sprite/" + Info.CharSprite);
    }
    
    public override void Attack()
    {  
        switch (Info.Equipment.Info.Gesture)
        {
            case ItemGesture.Swing:
                SetState<MobAttackSwing>();
                break;
            case ItemGesture.Shoot:
                SetState<MobAttackShoot>();
                break;
        }
    }

    /// <summary>Lock onto the nearest player or friendly NPC on sight; release it
    /// once it retreats well out of disengage range. Aggressive mobs call this from
    /// their OnUpdate; leashed mobs (GuardModule) skip re-acquiring while off
    /// leash or already heading home.</summary>
    protected virtual void UpdateAggro()
    {
        GuardModule guard = GetModule<GuardModule>();
        if (guard != null && (guard.IsBeyondLeash || IsCurrentState<MobReturnHome>())) return;

        Info nearest = FindNearestAggroTarget();
        if (nearest != null)
        {
            if (Info.Target != nearest)
            {
                Info.Target = nearest;
                Info.PathingStatus = PathingStatus.Pending;
            }
            return;
        }

        if (Info.Target != null &&
            Vector3.Distance(Info.Target.position, transform.position) > Info.DistDisengage)
            Info.CancelTarget();
    }

    /// <summary>Nearest player or friendly NPC within alert range.</summary>
    private Info FindNearestAggroTarget()
    {
        Info best = (Main.PlayerInfo != null && !Main.PlayerInfo.Destroyed &&
                     Vector3.Distance(Main.PlayerInfo.position, transform.position) <= Info.DistAlert)
            ? Main.PlayerInfo : null;
        Info npc = EntityScan.FindNearest(transform.position, Info.DistAlert, i => i is DynamicInfo d && d.IsNPC);
        if (npc != null && (best == null ||
            (npc.position - transform.position).sqrMagnitude < (best.position - transform.position).sqrMagnitude))
            best = npc;
        return best;
    }
     
}