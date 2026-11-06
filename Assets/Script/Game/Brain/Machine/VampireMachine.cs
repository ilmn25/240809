using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>A vampire enemy that flies like a harpy, chases the player, and
/// attacks with a melee bite. It can only be damaged by fire (see VampireInfo).
/// It teleports to dodge: after attacking, when hit, and periodically during
/// combat — always only once it's back to idle (never mid-animation).</summary>
public class VampireMachine : GroundMobMachine
{
    protected override bool UsesDoorBash => true;

    private static readonly ProjectileInfo BiteProjectile = new ContactDamageProjectileInfo {
        Damage = 4,
        Knockback = 10,
        CritChance = 0.15f,
        Radius = 0.9f,
    };

    private const float TeleportRange = 6f;      // how far it blinks when dodging
    private const int TeleportCooldown = 240;    // frames between periodic dodges (~4s)
    private const int TeleportOnHitCooldown = 90; // frames before it can dodge again after being hit

    private int _teleportTimer;
    private int _hitCooldown;
    private bool _pendingTeleport; // teleport once back to idle

    public static Info CreateInfo()
    {
        return new VampireInfo()
        {
            HealthMax = 40,
            DistAlert = 18,
            DistDisengage = 26,
            DistRoam = 7,
            PathJump = 10,
            PathAir = -1,
            SpeedGround = 6,
            SpeedAir = 8,
            JumpVelocity = 7,
            CanFly = true,
        };
    }

    public override void OnStart()
    {
        base.OnStart();

        AddState(new MobIdle());
        AddState(new MobChase());
        AddState(new MobRoam());
        AddState(new MobEvade());
        AddState(new MobHit());
        AddState(new MobAttackSwing(BiteProjectile));
        AddState(new EquipSelectState());
    }

    public override void OnUpdate()
    {
        if (_hitCooldown > 0) _hitCooldown--;

        // Teleport once we're back to idle — after an attack or hit animation.
        if (_pendingTeleport && IsCurrentState<DefaultState>())
        {
            _pendingTeleport = false;
            TeleportDodge();
            return;
        }

        if (!IsCurrentState<DefaultState>()) return;

        UpdatePlayerAggro();

        if (Info.Target == null)
        {
            RoamOrIdle();
            return;
        }

        // Periodically blink while the player is a threat.
        if (++_teleportTimer >= TeleportCooldown)
        {
            _teleportTimer = 0;
            TeleportDodge();
            return;
        }

        if (Vector3.Distance(Info.Target.position, transform.position) < Info.DistAttack)
            _pendingTeleport = true; // blink away after this attack finishes

        EngageTarget();
    }

    /// <summary>Called when the vampire is hit — it queues a teleport dodge that
    /// fires once the hit animation finishes (back in DefaultState).</summary>
    public void OnVampireHit()
    {
        if (_hitCooldown > 0) return;
        _hitCooldown = TeleportOnHitCooldown;
        _pendingTeleport = true;
    }

    /// <summary>Blink to a random nearby position (away from the target if possible),
    /// snapped to a valid surface so it never ends up inside the ground.</summary>
    private void TeleportDodge()
    {
        Vector3 pos = transform.position;
        Vector3 away = Info.Target != null
            ? (pos - Info.Target.position).normalized
            : Random.insideUnitSphere;
        away.y = 0;
        if (away.sqrMagnitude < 0.01f) away = Vector3.right;

        Vector3Int dest = Vector3Int.FloorToInt(pos + away * Random.Range(TeleportRange * 0.5f, TeleportRange));

        // Snap to the surface so we don't teleport into the ground.
        if (!NavMap.TryFindSurface(ref dest))
            return; // no valid spot — stay put

        transform.position = dest;
        Info.PathingStatus = PathingStatus.Pending;
        Particle.Create(dest + Vector3.up * 0.5f, Particles.Smoke, false);
    }
}
