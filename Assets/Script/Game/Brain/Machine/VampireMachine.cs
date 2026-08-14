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
        AddState(new MobAttackStopSwing(BiteProjectile));
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

        // Aggressive: lock onto the player on sight, drop them once they flee.
        bool playerAlive = Main.PlayerInfo != null && !Main.PlayerInfo.Destroyed;
        bool playerInRange = playerAlive &&
            Vector3.Distance(Main.PlayerInfo.position, transform.position) < Info.DistAlert;
        if (playerInRange && Info.Target != Main.PlayerInfo)
        {
            Info.Target = Main.PlayerInfo;
            Info.PathingStatus = PathingStatus.Pending;
        }
        else if (!playerInRange && Info.Target == Main.PlayerInfo &&
                 Vector3.Distance(Main.PlayerInfo.position, transform.position) > Info.DistDisengage)
        {
            Info.CancelTarget();
        }

        if (Info.Target == null)
        {
            if (Random.value > 0.5f)
                SetState<MobRoam>();
            else
                SetState<MobIdle>();
            return;
        }

        // Periodically blink while the player is a threat.
        if (++_teleportTimer >= TeleportCooldown)
        {
            _teleportTimer = 0;
            TeleportDodge();
            return;
        }

        float dist = Vector3.Distance(Info.Target.position, transform.position);
        if (dist < Info.DistAttack)
        {
            Info.AimPosition = Info.Target.position;
            _pendingTeleport = true; // blink away after this attack finishes
            SetState<MobAttackStopSwing>();
        }
        else if (Info.PathingStatus == PathingStatus.Stuck)
        {
            SetState<MobRoam>();
        }
        else
        {
            SetState<MobChase>();
        }
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
        dest.x = Mathf.Clamp(dest.x, 1, World.Inst.Bounds.x - 1);
        dest.z = Mathf.Clamp(dest.z, 1, World.Inst.Bounds.z - 1);

        // Snap down to the surface so we don't teleport into the ground.
        if (!FindSurfacePosition(ref dest))
            return; // no valid spot — stay put

        transform.position = dest;
        Info.PathingStatus = PathingStatus.Pending;
        Particle.Create(dest + Vector3.up * 0.5f, Particles.Smoke, false);
    }

    /// <summary>Scan downward from the given position to find the first air block
    /// directly above a solid block — the surface. Returns false if none found.</summary>
    private static bool FindSurfacePosition(ref Vector3Int pos)
    {
        int worldBottom = 0;
        int worldTop = World.Inst.Bounds.y;

        pos.x = Mathf.Clamp(pos.x, 0, World.Inst.Bounds.x - 1);
        pos.z = Mathf.Clamp(pos.z, 0, World.Inst.Bounds.z - 1);

        pos.y = worldTop - 1;
        while (pos.y > worldBottom)
        {
            bool currentAir = NavMap.Get(pos) == NavMap.Air;
            pos.y--;
            bool belowSolid = NavMap.Get(pos) != NavMap.Air;

            if (currentAir && belowSolid)
            {
                pos.y++;
                return true;
            }
        }
        return false;
    }
}
