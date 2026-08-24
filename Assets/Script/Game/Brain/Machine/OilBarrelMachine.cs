using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>A barrel of oil. It is flammable and, the moment it catches fire,
/// explodes: an area blast that damages everything nearby, shakes the screen,
/// and flings burning planks.</summary>
public class OilBarrelMachine : StructureMachine
{
    private const float ExplosionRadius = 3f;
    private const int ExplosionDamage = 14;
    private const float ExplosionKnockback = 12f;
    private const int PlankCount = 3;
    private const float FuseTime = 2f; // burn time before detonation

    private static readonly ExplosionProjectileInfo Explosion = new ExplosionProjectileInfo {
        Damage = ExplosionDamage,
        Knockback = ExplosionKnockback,
        Radius = ExplosionRadius,
        Sprite = ID.BulletProjectile,
        Class = ProjectileClass.Magic,
    };

    // Explosion projectiles need a source; this neutral stand-in lets the blast
    // damage anything (HitboxType.All) without an owning mob.
    private readonly MobInfo _source = CreateProjectileSource(HitboxType.All);
    private bool _exploded;
    private float _fireTime;

    public static Info CreateInfo()
    {
        return new StructureInfo
        {
            Health = 30,
            Loot = ID.OilBarrel,
            SfxHit = SfxID.HitMetal,
            SfxDestroy = SfxID.HitStone,
            operationType = OperationType.Mining,
            threshold = 1,
            SpawnsRubble = false,
        };
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        if (!Helper.IsHost()) return;
        if (_exploded || Info.Destroyed) return;
        if (Info.FireLevel <= 0f) return;

        // Burn for a short fuse before detonating, so there's a beat to react.
        _fireTime += Time.deltaTime;
        if (_fireTime >= FuseTime)
            Explode();
    }

    private void Explode()
    {
        _exploded = true;
        Vector3 pos = transform.position;

        Audio.PlaySFX(SfxID.Thunder);
        ScreenShake.Shake(80f, 0.4f, 0.5f);
        Particle.Create(pos, Particles.Fire, false);
        Particle.Create(pos, Particles.Smoke, false);
        Particle.Create(pos, Particles.HitDust, false);

        // Detach the barrel first so the blast doesn't break it (or re-drop its loot).
        Info.Destroy();

        Projectile.Spawn(pos, pos, Explosion, HitboxType.All, _source);

        for (int i = 0; i < PlankCount; i++)
        {
            Vector3 dir = Random.insideUnitCircle;
            Vector3 velocity = new Vector3(dir.x, Random.Range(1f, 3f), dir.y) * 3f;
            Entity.SpawnItem(ID.Plank, pos, 1, false, velocity, 2400);
            Particle.Create(pos, Particles.Fire, false);
        }
    }
}
