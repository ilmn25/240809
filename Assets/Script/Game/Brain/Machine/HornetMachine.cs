using UnityEngine;

/// <summary>Flying hive-guard that stings anything near it with a dodgable swing.</summary>
public class HornetMachine : GroundMobMachine
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
            DistDisengage = 7,
            DistRoam = 5,
            PathJump = 10,
            PathAir = -1,
            SpeedGround = 4f,
            SpeedAir = 4f,
            JumpVelocity = 7f,
            CanFly = true,
        };
    }

    public override void OnStart()
    {
        Info.targetHitboxType = HitboxType.Friendly; // sting players, NPCs and animals
        base.OnStart();

        AddState(new MobIdle());
        AddState(new MobChase());
        AddState(new MobRoam());
        AddState(new MobHit());
        AddState(new MobAttackSwing(StingProjectile));
    }

    public override void OnUpdate()
    {
        if (!IsCurrentState<DefaultState>()) return;

        UpdateThreat(IsThreat);
        if (!EngageTarget()) RoamOrIdle();
    }

    private static bool IsThreat(Info i)
    {
        if (i is EnemyInfo) return false; // don't sting fellow hornets/hostiles
        return i is DynamicInfo d && d.Health > 0;
    }
}
