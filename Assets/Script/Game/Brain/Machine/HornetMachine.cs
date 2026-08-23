using UnityEngine;

/// <summary>Flying hive-guard that stings anything near it with a dodgable swing.</summary>
public class HornetMachine : FlyingEnemyMachine
{
    private static readonly ProjectileInfo StingProjectile = new SwingProjectileInfo {
        Damage = 1,
        Knockback = 5,
        CritChance = 0.1f,
        Radius = 1f,
    };

    public static Info CreateInfo()
    {
        return new EnemyInfo()
        {
            HealthMax = 3,
            DistAttack = 2,
            DistAlert = 7,
            DistDisengage = 10,
            SpeedGround = 0,
            SpeedAir = 8f,
            CanFly = true,
        };
    }

    public override void OnStart()
    {
        Info.targetHitboxType = HitboxType.Friendly; // sting players, NPCs and animals
        base.OnStart();
        AddState(new MobAttackStopSwing(StingProjectile));
    }

    protected override bool IsThreat(Info i)
    {
        if (i is EnemyInfo) return false; // don't sting fellow hornets/hostiles
        return i is DynamicInfo d && d.Health > 0;
    }

    protected override void AttackTarget()
    {
        Info.AimPosition = Info.Target.position;
        SetState<MobAttackStopSwing>();
    }
}
