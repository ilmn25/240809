using UnityEngine;

/// <summary>Cyclops — a hulking one-eyed brute and the chest-guardian boss
/// (think the Ancient Guardian standing over its ornate chest). It holds its
/// post beside a sealed chest, aggroes anyone who comes to loot, and only
/// returns home when dragged off its leash. Slaying it opens the chest.</summary>
public class CyclopsMachine : HostileMeleeMachine
{
    private static readonly ProjectileInfo SmashProjectile = new ContactDamageProjectileInfo {
        Damage = 12,
        Knockback = 40,
        CritChance = 0.1f,
        Radius = 1.4f,
    };

    protected override ProjectileInfo AttackProjectile => SmashProjectile;

    /// <summary>World position of the sealed chest this Cyclops guards.</summary>
    public Vector3 HomePosition
    {
        get => GetModule<GuardModule>().HomePosition;
        set => GetModule<GuardModule>().HomePosition = value;
    }

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
            DistRoam = 4,      // keeps a tight patrol around the chest
            SpeedGround = 0.6f,
            SpeedAir = 1.0f,
            SpeedLogic = 0.8f,
            PathJump = 1,
            PathAir = 3,
        };
    }

    public override void OnStart()
    {
        base.OnStart();
        AddModule(new GuardModule());
        AddState(new MobReturnHome());
    }

    /// <summary>Don't re-acquire a target while dragged off the leash or already
    /// heading home — that fights the return-home pathing and freezes the guard.</summary>
    protected override void UpdateAggro()
    {
        GuardModule guard = GetModule<GuardModule>();
        if (guard.IsBeyondLeash || IsCurrentState<MobReturnHome>()) return;
        base.UpdateAggro();
    }
}
