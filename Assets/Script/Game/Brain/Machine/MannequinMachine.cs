using UnityEngine;

/// <summary>Mannequin enemy — a weeping-angel style stalker. It darts toward the
/// player at high speed while unobserved, but freezes in place the instant the
/// player is looking at it. Once the player looks away, it resumes its rush.
/// Its attack is a stop-swing that deals 3 damage.</summary>
public class MannequinMachine : GroundMobMachine
{
    protected override bool UsesDoorBash => true;

    private const float FaceThreshold = 0.35f;   // dot product that counts as "being faced"
    private const float FastSpeed = 13f;          // rapid approach while unobserved

    private static readonly ProjectileInfo StrikeProjectile = new ContactDamageProjectileInfo {
        Damage = 3,
        Knockback = 6,
        CritChance = 0.1f,
        Radius = 0.7f,
    };

    public static Info CreateInfo()
    {
        return new EnemyInfo()
        {
            HealthMax = 22,
            DistAttack = 2,
            DistAlert = 20,
            DistDisengage = 20,
            DistRoam = 4,
            SpeedGround = FastSpeed,
            SpeedAir = FastSpeed,
            SpeedLogic = FastSpeed,
            PathJump = 2,
            PathAir = 4,
        };
    }

    public override void OnStart()
    {
        base.OnStart();

        AddState(new MobIdle());
        AddState(new MobChase());
        AddState(new MobRoam());
        AddState(new MobHit());
        AddState(new MobAttackStopSwing(StrikeProjectile));
        AddState(new MobMannequinFrozen());
        AddState(new EquipSelectState());
    }

    public override void OnUpdate()
    {
        bool playerAlive = Main.PlayerInfo != null && !Main.PlayerInfo.Destroyed;
        float playerDist = playerAlive
            ? Vector3.Distance(Main.PlayerInfo.position, transform.position)
            : float.MaxValue;

        // Weeping-angel freeze: while the player looks at us, we can neither move
        // nor attack.
        if (playerAlive && IsBeingFaced())
        {
            if (!IsCurrentState<MobMannequinFrozen>())
                SetState<MobMannequinFrozen>();
            return;
        }

        // Unobserved again — resume the hunt.
        if (IsCurrentState<MobMannequinFrozen>())
            SetState<DefaultState>();

        bool playerInRange = playerAlive && playerDist < Info.DistAlert;
        if (playerInRange && Info.Target != Main.PlayerInfo)
        {
            Info.Target = Main.PlayerInfo;
            Info.PathingStatus = PathingStatus.Pending;
        }
        else if (!playerInRange && Info.Target == Main.PlayerInfo && playerDist > Info.DistDisengage)
        {
            Info.CancelTarget();
        }

        if (IsCurrentState<DefaultState>())
        {
            if (Info.Target != null)
            {
                if (playerDist < Info.DistAttack)
                {
                    Info.AimPosition = Info.Target.position;
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
            else if (Random.value > 0.5f)
                SetState<MobRoam>();
            else
                SetState<MobIdle>();
        }
    }

    /// <summary>True when the player's facing (mouse) direction points at us —
    /// i.e. the player is looking at the mannequin.</summary>
    private bool IsBeingFaced()
    {
        PlayerInfo player = Main.PlayerInfo;
        if (player == null || player.Machine == null || Camera.main == null) return false;

        Vector3 screenDir = Camera.main.WorldToScreenPoint(transform.position)
                          - Camera.main.WorldToScreenPoint(player.position);
        screenDir.z = 0;
        Vector2 toMe = screenDir.normalized;
        Vector2 facing = player.TargetScreenDir;
        return Vector2.Dot(facing, toMe) > FaceThreshold;
    }

    /// <summary>Stops all movement and pathing. Used while the player is looking
    /// at the mannequin, so it stands perfectly still.</summary>
    private class MobMannequinFrozen : MobState
    {
        public override void OnEnterState()
        {
            Info.PathingStatus = PathingStatus.Stuck;
            Info.Direction = Vector3.zero;
            Info.SpeedTarget = 0;
            Info.SpeedCurrent = 0;
        }

        public override void OnUpdateState()
        {
            Info.PathingStatus = PathingStatus.Stuck;
            Info.Direction = Vector3.zero;
            Info.SpeedTarget = 0;
            Info.SpeedCurrent = 0;
        }
    }
}