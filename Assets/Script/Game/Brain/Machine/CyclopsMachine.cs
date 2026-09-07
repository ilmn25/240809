using UnityEngine;

/// <summary>Cyclops — a hulking one-eyed brute and the chest-guardian boss
/// (think the Ancient Guardian standing over its ornate chest). It's a plain
/// hostile melee mob: it aggroes on sight and freezes to swing when it closes to
/// melee range. The chest it guards tracks whether it's still alive and opens
/// (unlocks) once it falls — see CyclopsChestMachine.</summary>
public class CyclopsMachine : HostileMeleeMachine
{
    private static readonly ProjectileInfo SmashProjectile = new ContactDamageProjectileInfo {
        Damage = 12,
        Knockback = 40,
        CritChance = 0.1f,
        Radius = 1.4f,
    };

    protected override ProjectileInfo AttackProjectile => SmashProjectile;

    public static Info CreateInfo()
    {
        return new EnemyInfo()
        {
            HealthMax = 400,
            // No dedicated sprite yet — reuse the lich's look. Once a
            // "Sprite/Cyclops" texture exists, drop this to default to it.
            CharSprite = ID.Lich,
            DistAttack = 2,
            DistAlert = 10,    // notices looters from a ways off
            DistDisengage = 16,
            DistRoam = 4,
            SpeedGround = 0.6f,
            SpeedAir = 1.0f,
            SpeedLogic = 0.8f,
            PathJump = 1,
            PathAir = 3,
        };
    }
}
