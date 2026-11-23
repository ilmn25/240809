using UnityEngine;
using Random = UnityEngine.Random;

public class LandmineMachine : StructureMachine
{
    private const float ArmingDelay = 0.8f;
    private const int CheckInterval = 8;
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
        ShakeSpeed = 100f,
        ShakeStrength = 0.5f,
        ShakeDuration = 0.6f,
    };

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
            operationType = OperationType.Dismantling,
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

    public void OnBlastDestroyed()
    {
        if (_detonated || Time.time < _armedAt) return;
        Explode();
    }

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

        Info.Destroy();

        Projectile.Spawn(pos, pos + Vector3.forward, Explosion, HitboxType.All, _source);
    }
}
