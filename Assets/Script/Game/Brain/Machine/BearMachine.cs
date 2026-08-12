using UnityEngine;

/// <summary>Bear enemy: aggressively chases its target down, then freezes in place
/// to swing when it closes to melee range (hound-in-Dont-Starve behavior).</summary>
public class BearMachine : HostileMeleeMachine
{
    private static readonly ProjectileInfo ClawProjectile = new ContactDamageProjectileInfo {
        Damage = 3,
        Knockback = 14,
        CritChance = 0.1f,
        Radius = 0.9f,
    };

    protected override ProjectileInfo AttackProjectile => ClawProjectile;

    public static Info CreateInfo()
    {
        return new EnemyInfo()
        {
            HealthMax = 42,
            DistAttack = 2,
            DistAlert = 16,
            DistDisengage = 18,
            DistRoam = 6,
            SpeedGround = 4,
            SpeedAir = 6,
            PathJump = 2,
            PathAir = 4,
        };
    }
}