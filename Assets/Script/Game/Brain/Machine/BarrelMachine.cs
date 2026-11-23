using UnityEngine;
using Random = UnityEngine.Random;

public class BarrelMachine : StructureMachine, IActionSecondaryInteract
{
    private const float ExplosionRadius = 3f;
    private const int ExplosionDamage = 14;
    private const float ExplosionKnockback = 12f;
    private const int PlankCount = 3;
    private const float FuseTime = 2f;

    private static readonly ExplosionProjectileInfo Explosion = new ExplosionProjectileInfo {
        Damage = ExplosionDamage,
        Knockback = ExplosionKnockback,
        Radius = ExplosionRadius,
        Sprite = ID.BulletProjectile,
        Class = ProjectileClass.Magic,
    };

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

        _fireTime += Time.deltaTime;
        if (_fireTime >= FuseTime)
            Explode();
    }

    public void OnActionSecondary(Info info)
    {
        string contents = Barrell == null || Barrell.Liquid == LiquidType.None
            ? "empty"
            : Barrell.Liquid.ToString().ToLower();
        Dialogue.ShowEvent(contents == "empty" ? "An empty barrel." : $"A barrel of {contents}.");
    }

    public bool TryCollect(MobInfo source)
    {
        if (Info.Destroyed || Barrell == null || Barrell.Liquid == LiquidType.None) return false;
        if (!LiquidRegistry.TryFillBucket(source, Barrell.Liquid, transform.position)) return false;

        if (Info.id == ID.OilBarrel)
            ConvertToPlainBarrel();
        else
            SetLiquid(LiquidType.None);
        return true;
    }

    public bool TryPour(MobInfo source)
    {
        if (Info.Destroyed || Barrell == null || Barrell.Liquid != LiquidType.None) return false;
        if (source is not PlayerInfo player || player.Storage == null)
            return false;

        ItemSlot held = player.HeldSlot;
        if (held is not { Stack: > 0 }) return false;
        if (!LiquidRegistry.TryGetLiquidFromBucket(held.ID, out LiquidType liquid)) return false;

        Inventory.ReplaceHeld(player, ID.Bucket);
        SetLiquid(liquid);
        Particle.Create(transform.position + Vector3.up * 0.5f, Particles.HitDust, false);
        return true;
    }

    private void SetLiquid(LiquidType liquid)
    {
        if (Barrell == null) return;
        Barrell.Liquid = liquid;
        BarrelSync.Send(this, Barrell);
    }

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

        Info.Destroy();

        Projectile.Spawn(pos, pos + Vector3.forward, Explosion, HitboxType.All, _source);

        for (int i = 0; i < PlankCount; i++)
        {
            Vector3 dir = Random.insideUnitCircle;
            Vector3 velocity = new Vector3(dir.x, Random.Range(1f, 3f), dir.y) * 3f;
            Entity.SpawnItem(ID.Plank, pos, 1, false, velocity, 2400);
            Particle.Create(pos, Particles.Fire, false);
        }
    }
}
