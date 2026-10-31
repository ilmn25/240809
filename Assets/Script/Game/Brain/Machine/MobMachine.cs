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

    /// <summary>Lock onto the player on sight and drop them once they retreat past
    /// disengage range. For mobs that ignore NPCs; <see cref="UpdateAggro"/> also picks
    /// up friendly NPCs.</summary>
    protected void UpdatePlayerAggro()
    {
        PlayerInfo player = Main.PlayerInfo;
        bool alive = player != null && !player.Destroyed;
        bool inRange = alive && Vector3.Distance(player.position, transform.position) < Info.DistAlert;

        if (inRange && Info.Target != player)
        {
            Info.Target = player;
            Info.PathingStatus = PathingStatus.Pending;
        }
        else if (!inRange && Info.Target == player &&
                 Vector3.Distance(player.position, transform.position) > Info.DistDisengage)
            Info.CancelTarget();
    }

    /// <summary>Next step of a melee engagement: swing when the target is in reach, roam
    /// when pathing is stuck, otherwise chase. False when there is no target.</summary>
    protected bool EngageTarget()
    {
        if (Info.Target == null) return false;

        if (Vector3.Distance(Info.Target.position, transform.position) < Info.DistAttack)
        {
            Info.AimPosition = Info.Target.position;
            SetState<MobAttackSwing>();
        }
        else if (Info.PathingStatus == PathingStatus.Stuck)
            SetState<MobRoam>();
        else
            SetState<MobChase>();
        return true;
    }

    /// <summary>Wander when there is nothing to fight.</summary>
    protected void RoamOrIdle()
    {
        if (Random.value > 0.5f) SetState<MobRoam>();
        else SetState<MobIdle>();
    }
     
}