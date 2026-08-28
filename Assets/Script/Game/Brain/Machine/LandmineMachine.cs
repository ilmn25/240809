using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>A pressure-plate mine. Once armed, any creature that steps into its
/// block detonates it: a violent blast that damages creatures and structures,
/// shakes the screen, and consumes the mine. Breaking it with a hammer safely
/// recovers it.</summary>
public class LandmineMachine : StructureMachine
{
    private const float ArmingDelay = 0.8f;   // seconds before the mine can detonate
    private const int CheckInterval = 8;      // frames between trigger checks
    private const float TriggerRadius = 0.55f;
    private const float ExplosionRadius = 3.5f;
    private const int ExplosionDamage = 25;
    private const float ExplosionKnockback = 14f;

    private static readonly ExplosionProjectileInfo Explosion = new ExplosionProjectileInfo {
        Damage = ExplosionDamage,
        Knockback = ExplosionKnockback,
        Radius = ExplosionRadius,
        Sprite = ID.BulletProjectile,
        Class = ProjectileClass.Magic,
    };

    // Neutral stand-in source so the blast damages anything (HitboxType.All).
    private readonly MobInfo _source = CreateProjectileSource(HitboxType.All);

    private static readonly Collider[] CollisionArray = new Collider[24];
    private float _armedAt;
    private int _timer;
    private bool _detonated;

    public static Info CreateInfo()
    {
        return new LandmineInfo
        {
            Health = 20,
            Loot = ID.Landmine,
            SfxHit = SfxID.HitMetal,
            SfxDestroy = SfxID.HitStone,
            operationType = OperationType.Building,
            threshold = 1,
            SpawnsRubble = false,
        };
    }

    public override void OnStart()
    {
        base.OnStart();
        _armedAt = Time.time + ArmingDelay;
        _timer = Random.Range(0, CheckInterval);
        _detonated = false;
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        if (!Helper.IsHost()) return;
        if (Info.Destroyed || _detonated) return;
        if (Time.time < _armedAt) return;
        if (++_timer < CheckInterval) return;
        _timer = 0;

        if (CreatureInArea())
            Explode();
    }

    /// <summary>Chain reaction hook called by LandmineInfo.OnDestroy when a blast
    /// (no attacker) destroys this armed mine — it detonates too, so a minefield
    /// goes up like dominoes.</summary>
    public void OnBlastDestroyed()
    {
        if (_detonated || Time.time < _armedAt) return;
        Explode();
    }

    /// <summary>True when a living creature (mob or player) is standing in the
    /// mine's block. Dropped items and other structures don't count.</summary>
    private bool CreatureInArea()
    {
        Vector3 center = transform.position + Vector3.up * 0.35f;
        int count = Physics.OverlapSphereNonAlloc(center, TriggerRadius, CollisionArray, Main.MaskEntity);
        for (int i = 0; i < count; i++)
        {
            Collider col = CollisionArray[i];
            if (col.gameObject == gameObject) continue;
            Machine machine = col.GetComponent<Machine>();
            if (machine == null) continue;
            if (machine.GetModule<Info>() is DynamicInfo) return true;
        }
        return false;
    }

    private void Explode()
    {
        _detonated = true;
        Vector3 pos = transform.position;

        Audio.PlaySFX(SfxID.Thunder);
        ScreenShake.Shake(100f, 0.5f, 0.6f);
        Particle.Create(pos, Particles.Fire, false);
        Particle.Create(pos, Particles.Smoke, false);
        Particle.Create(pos, Particles.HitDust, false);

        // Consume the mine first so the blast doesn't break it (or drop its loot).
        Info.Destroy();

        // +forward so Direction isn't zero (avoids a LookRotation warning);
        // direction is irrelevant for an instant area blast.
        Projectile.Spawn(pos, pos + Vector3.forward, Explosion, HitboxType.All, _source);
    }
}
