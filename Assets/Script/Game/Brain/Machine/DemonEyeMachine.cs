using UnityEngine;

/// <summary>Flying eyeball that homes onto the player and bites with contact damage.</summary>
public class DemonEyeMachine : FlyingEnemyMachine
{
    private static readonly ProjectileInfo TouchProjectile = new ContactDamageProjectileInfo {
        Damage = 3,
        Knockback = 8,
        CritChance = 0.1f,
        Radius = 0.8f,
    };

    public static Info CreateInfo()
    {
        return new EnemyInfo()
        {
            HealthMax = 24,
            DistAttack = 1,
            DistAlert = 7,
            DistDisengage = 10,
            SpeedGround = 0,
            SpeedAir = 4,
            CanFly = true,
        };
    }

    protected override bool IsThreat(Info i) =>
        i is PlayerInfo p && !p.Destroyed;

    protected override void AttackTarget()
    {
        Projectile.Spawn(transform.position, Info.Target.position, TouchProjectile, Info.targetHitboxType, Info);
    }
}
