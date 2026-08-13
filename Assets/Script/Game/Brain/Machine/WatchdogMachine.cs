using UnityEngine;

/// <summary>Watchdog enemy: like a bear (aggressively chases, freezes to swing in
/// melee range) but 1 unit faster and with less damage. It also predicts melee
/// attacks and dodges back out of range via the shared MeleeDodgeModule.</summary>
public class WatchdogMachine : HostileMeleeMachine
{
    private static readonly ProjectileInfo BiteProjectile = new ContactDamageProjectileInfo {
        Damage = 2,          // less than the bear's 3
        Knockback = 12,
        CritChance = 0.1f,
        Radius = 0.9f,
    };

    protected override ProjectileInfo AttackProjectile => BiteProjectile;

    public static Info CreateInfo()
    {
        return new EnemyInfo()
        {
            HealthMax = 42,
            DistAttack = 2,
            DistAlert = 16,
            DistDisengage = 18,
            DistRoam = 6,
            SpeedGround = 5,   // bear is 4 — 1 unit faster
            SpeedAir = 7,      // bear is 6 — 1 unit faster
            PathJump = 2,
            PathAir = 4,
            CharSprite = ID.Bear, // no dedicated sprite — reuse the bear
        };
    }

    public override void OnStart()
    {
        base.OnStart();
        AddModule(new MeleeDodgeModule());
    }
}
