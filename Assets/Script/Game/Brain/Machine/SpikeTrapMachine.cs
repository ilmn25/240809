using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>A floor trap of sharpened stakes. Anything that steps on it takes
/// repeated contact damage. Wood and metal variants share this machine; the
/// metal one hits harder and is tougher. Semi-collide so creatures path right
/// over it (and get stabbed) instead of being blocked.</summary>
public class SpikeTrapMachine : StructureMachine
{
    private const int HitInterval = 30;   // frames between damage checks (~0.5s at 60fps)
    private const float HitRadius = 0.55f; // small sphere over the trap: catches creatures standing on it, not beside it
    private int _timer;

    private static readonly ContactDamageProjectileInfo WoodHit = new ContactDamageProjectileInfo {
        Damage = 2,
        Knockback = 6,
        CritChance = 0,
        Radius = HitRadius,
        Class = ProjectileClass.Melee,
    };
    private static readonly ContactDamageProjectileInfo MetalHit = new ContactDamageProjectileInfo {
        Damage = 4,
        Knockback = 8,
        CritChance = 5,
        Radius = HitRadius,
        Class = ProjectileClass.Melee,
    };

    // Spikes stab creatures only. A non-Enemy source means the contact hit can't
    // break structures (StructureInfo.CanBreak needs an Enemy source or a tool),
    // and targetHitboxType = All means any creature (mob, player, NPC) is hurt.
    private readonly MobInfo _source = new MobInfo { HitboxType = HitboxType.Friendly, targetHitboxType = HitboxType.All };

    public static Info CreateInfo()
    {
        return new StructureInfo
        {
            Health = 30,
            SfxHit = SfxID.HitStone,
            SfxDestroy = SfxID.HitStone,
            operationType = OperationType.Building,
            threshold = 1,
            SpawnsRubble = false,
        };
    }

    public override void OnStart()
    {
        base.OnStart();
        _timer = Random.Range(0, HitInterval); // stagger so nearby traps don't all fire together
        if (Info is StructureInfo si)
        {
            // Loot/health depend on which variant this is (CreateInfo runs before the id is known).
            bool metal = Info.id == ID.MetalSpikeTrap;
            si.Loot = Info.id;
            si.Health = metal ? 70 : 30;
        }
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        if (!Helper.IsHost()) return;
        if (Info.Destroyed) return;
        if (++_timer < HitInterval) return;
        _timer = 0;

        ContactDamageProjectileInfo hit = Info.id == ID.MetalSpikeTrap ? MetalHit : WoodHit;
        // Spawn slightly above the floor to center the contact sphere on the
        // creatures standing on the trap.
        Vector3 pos = transform.position + Vector3.up * 0.35f;
        Projectile.Spawn(pos, pos + Vector3.up, hit, HitboxType.All, _source);
    }
}
