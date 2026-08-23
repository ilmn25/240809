using UnityEngine;

/// <summary>Shared brain for flying enemies that hover toward a target without
/// ground pathing. Owns the hover module stack, target acquisition, and the
/// in-range attack trigger; subclasses supply the target filter and the attack.</summary>
public abstract class FlyingEnemyMachine : MobMachine
{
    protected abstract bool IsThreat(Info i);
    protected abstract void AttackTarget();

    public override void OnStart()
    {
        AddModule(new HoverMovementModule(hoverHeight: 0.5f, stopDistance: 0.9f, turnSpeed: 2f));
        AddModule(new MobSpriteCullModule());
        AddModule(new SpriteOrbitModule());
        AddState(new MobHit());
    }

    public override void OnUpdate()
    {
        if (!IsCurrentState<DefaultState>()) return;

        Info nearest = EntityScan.FindNearest(transform.position, Info.DistAlert, IsThreat);
        if (nearest != null && nearest != Info.Target)
        {
            Info.Target = nearest;
            Info.PathingStatus = PathingStatus.Pending;
        }
        else if (nearest == null && Info.Target != null &&
                 Vector3.Distance(Info.Target.position, transform.position) > Info.DistDisengage)
        {
            Info.CancelTarget();
        }

        if (Info.Target != null &&
            Vector3.Distance(Info.Target.position, transform.position) < Info.DistAttack)
            AttackTarget();
    }
}
