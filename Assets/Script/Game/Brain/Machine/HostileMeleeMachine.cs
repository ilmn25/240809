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
}
