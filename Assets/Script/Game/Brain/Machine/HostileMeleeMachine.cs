using UnityEngine;

/// <summary>Base for hostile melee mobs that chase the player down and freeze to
/// swing when in range. Provides the shared door-bashing module stack, the
/// standard melee states, the aggro-on-sight target acquisition, and the common
/// chase → stop-and-swing brain. Subclasses only supply the attack projectile
/// (and any special-case OnUpdate hooks).</summary>
public abstract class HostileMeleeMachine : GroundMobMachine
{
    protected override bool UsesDoorBash => true;

    /// <summary>The melee projectile swung at the target.</summary>
    protected abstract ProjectileInfo AttackProjectile { get; }

    public override void OnStart()
    {
        base.OnStart();
        AddState(new MobIdle());
        AddState(new MobChase());
        AddState(new MobRoam());
        AddState(new MobHit());
        AddState(new MobAttackStopSwing(AttackProjectile));
        AddState(new EquipSelectState());
    }

    public override void OnUpdate()
    {
        UpdateAggro();

        if (!IsCurrentState<DefaultState>())
            return;

        if (Info.Target != null)
        {
            if (Vector3.Distance(Info.Target.position, transform.position) < Info.DistAttack)
            {
                Info.AimPosition = Info.Target.position;
                SetState<MobAttackStopSwing>();
            }
            else if (Info.PathingStatus == PathingStatus.Stuck)
                SetState<MobRoam>();
            else
                SetState<MobChase>();
        }
        else if (Random.value > 0.5f)
            SetState<MobRoam>();
        else
            SetState<MobIdle>();
    }

    /// <summary>Lock onto the nearest player or friendly NPC on sight; release it
    /// once it retreats well out of disengage range.</summary>
    protected void UpdateAggro()
    {
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

        // Nothing in range — release any current target that has wandered off.
        if (Info.Target != null &&
            Vector3.Distance(Info.Target.position, transform.position) > Info.DistDisengage)
            Info.CancelTarget();
    }

    private Info FindNearestAggroTarget()
    {
        // The player is always a candidate (relentless); friendly NPCs only within
        // alert range.
        Info best = (Main.PlayerInfo != null && !Main.PlayerInfo.Destroyed) ? Main.PlayerInfo : null;
        Info npc = EntityScan.FindNearest(transform.position, Info.DistAlert, i => i is DynamicInfo d && d.IsNPC);
        if (npc != null && (best == null ||
            (npc.position - transform.position).sqrMagnitude < (best.position - transform.position).sqrMagnitude))
            best = npc;
        return best;
    }
}
