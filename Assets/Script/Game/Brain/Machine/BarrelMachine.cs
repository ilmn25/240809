using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>A barrel that holds a liquid. A barrel filled with oil is flammable
/// and, the moment it catches fire, explodes: an area blast that damages
/// everything nearby, shakes the screen, and flings burning planks. Swinging an
/// empty bucket at a filled barrel collects its liquid into the bucket.</summary>
public class BarrelMachine : StructureMachine, IActionSecondaryInteract
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

    private BarrelInfo Barrell => Info as BarrelInfo;

    public static Info CreateInfo()
    {
        return new BarrelInfo
        {
            Health = 30,
            SfxHit = SfxID.HitMetal,
            SfxDestroy = SfxID.HitStone,
            operationType = OperationType.Mining,
            threshold = 1,
            SpawnsRubble = false,
        };
    }

    public override void OnStart()
    {
        base.OnStart();
        // Runs on every spawn (pooled barrels are reused), so re-derive content
        // and clear per-lifecycle state that a previous occupant may have left.
        // Oil barrels saved before this refactor deserialize as plain StructureInfo.
        if (Barrell == null) return;
        Barrell.Loot = Info.id;
        if (Info.id == ID.OilBarrel)
            Barrell.Liquid = LiquidType.Oil;
        _exploded = false;
        _fireTime = 0f;
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        if (!Helper.IsHost()) return;
        if (_exploded || Info.Destroyed) return;
        if (Barrell == null || Barrell.Liquid != LiquidType.Oil) return;
        if (Info.FireLevel <= 0f) return;

        // Burn for a short fuse before detonating, so there's a beat to react.
        _fireTime += Time.deltaTime;
        if (_fireTime >= FuseTime)
            Explode();
    }

    /// <summary>Right-click a barrel to inspect its contents (the bucket collect
    /// is the primary swing action).</summary>
    public void OnActionSecondary(Info info)
    {
        string contents = Barrell == null || Barrell.Liquid == LiquidType.None
            ? "empty"
            : Barrell.Liquid.ToString().ToLower();
        Dialogue.ShowEvent(contents == "empty" ? "An empty barrel." : $"A barrel of {contents}.");
    }

    /// <summary>Collect the barrel's liquid into the player's empty bucket.
    /// Returns true when the swing was consumed (bucket filled).</summary>
    public bool TryCollect(MobInfo source)
    {
        if (Info.Destroyed || Barrell == null || Barrell.Liquid == LiquidType.None) return false;
        if (!LiquidRegistry.TryFillBucket(source, Barrell.Liquid, transform.position)) return false;

        // An oil barrel empties into a plain barrel; any other barrel just drains.
        if (Info.id == ID.OilBarrel)
            ConvertToPlainBarrel();
        else
            SetLiquid(LiquidType.None);
        return true;
    }

    /// <summary>Pour a held filled bucket's contents into an empty barrel.
    /// Returns true when the swing was consumed (bucket emptied).</summary>
    public bool TryPour(MobInfo source)
    {
        if (Info.Destroyed || Barrell == null || Barrell.Liquid != LiquidType.None) return false;
        if (source is not PlayerInfo player ||
            player.Storage?.List == null || player.Storage.List.Count == 0)
            return false;

        ItemSlot selected = player.Storage.GetSelected();
        if (selected == null || selected.Stack <= 0) return false;
        if (!LiquidRegistry.TryGetLiquidFromBucket(selected.ID, out LiquidType liquid)) return false;

        player.Storage.List[player.Storage.Key] = new ItemSlot(ID.Bucket, 1);
        player.Storage.NotifyChanged();
        SetLiquid(liquid);
        Particle.Create(transform.position + Vector3.up * 0.5f, Particles.HitDust, false);
        return true;
    }

    /// <summary>Update the barrel's stored liquid locally and sync it.</summary>
    private void SetLiquid(LiquidType liquid)
    {
        if (Barrell == null) return;
        Barrell.Liquid = liquid;
        BarrelSync.Send(this, Barrell);
    }

    /// <summary>Apply an authoritative liquid change (from host broadcast or
    /// client relay). Does not re-broadcast to avoid loops.</summary>
    public void ApplyLiquid(LiquidType liquid)
    {
        if (Barrell != null)
            Barrell.Liquid = liquid;
    }

    private void ConvertToPlainBarrel()
    {
        Vector3Int pos = Vector3Int.FloorToInt(transform.position);
        Info.Destroy();
        Entity.Spawn(ID.Barrel, pos);
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
