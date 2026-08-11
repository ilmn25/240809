using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>Spider enemy: fast, erratic hunter that periodically strafes while
/// closing in, then freezes to swing when in melee range. Spawned by SpiderNestMachine.</summary>
public class SpiderMachine : MobMachine
{
    private static readonly ProjectileInfo BiteProjectile = new ContactDamageProjectileInfo {
        Damage = 2,
        Knockback = 8,
        CritChance = 0.1f,
        Radius = 0.7f,
    };

    private int _strafeTimer;
    private int _webTimer;
    private int _websLaid;                 // how many webs this spider has laid
    private const int MaxWebs = 3;         // hard cap per spider

    public static Info CreateInfo()
    {
        return new EnemyInfo()
        {
            HealthMax = 18,
            DistAttack = 2,
            DistAlert = 14,
            DistDisengage = 18,
            DistRoam = 5,
            DistStrafe = 3,
            SpeedGround = 5.5f,
            SpeedAir = 7,
            PathJump = 2,
            PathAir = 4,
        };
    }

    public override void OnStart()
    {
        AddModule(new GroundMovementModule());
        AddModule(new GroundPathingModule());
        AddModule(new GroundAnimationModule());
        AddModule(new MobSpriteCullModule());
        AddModule(new SpriteOrbitModule());
        AddModule(new DoorBashModule());

        AddState(new MobIdle());
        AddState(new MobChase());
        AddState(new MobStrafe());
        AddState(new MobRoam());
        AddState(new MobEvade());
        AddState(new MobHit());
        AddState(new MobAttackStopSwing(BiteProjectile));
        AddState(new EquipSelectState());
    }

    /// <summary>Alerted by a web: head toward the player who stepped on it.</summary>
    public void Investigate(Info target)
    {
        if (Info.Target != null) return; // already hunting something
        Info.Target = target;
        Info.PathingStatus = PathingStatus.Pending;
        SetState<MobChase>();
    }

    public override void OnUpdate()
    {
        // Lay a web behind us occasionally while active, up to a per-spider cap.
        // Lay a web in the morning only (first 8 hours of the day), up to a per-spider cap.
        bool isMorning = Save.Inst.time < 60 * 8;
        if (Helper.IsHost() && isMorning && _websLaid < MaxWebs && ++_webTimer >= 900 && Random.value < 0.4f)
        {
            _webTimer = 0;
            Vector3Int webPos = Vector3Int.FloorToInt(transform.position);
            if (World.GetBlock(webPos) == 0)
            {
                Entity.Spawn(ID.SpiderWeb, webPos);
                _websLaid++;
            }
        }

        bool playerAlive = Main.PlayerInfo != null && !Main.PlayerInfo.Destroyed;
        bool playerInRange = playerAlive &&
            Vector3.Distance(Main.PlayerInfo.position, transform.position) < Info.DistAlert;

        // Aggressive: mark the player as a target on sight, drop it once they flee.
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

        if (IsCurrentState<DefaultState>())
        {
            if (Info.Target != null)
            {
                if (Vector3.Distance(Info.Target.position, transform.position) < Info.DistAttack)
                {
                    Info.AimPosition = Info.Target.position;
                    SetState<MobAttackStopSwing>();
                }
                else if (Info.PathingStatus == PathingStatus.Stuck)
                {
                    SetState<MobRoam>();
                }
                // Erratic: dart sideways every so often while closing in.
                else if (++_strafeTimer > 90 && Random.value < 0.5f)
                {
                    _strafeTimer = 0;
                    SetState<MobStrafe>();
                }
                else
                {
                    SetState<MobChase>();
                }
            }
            else
            {
                if (Random.value > 0.5f)
                    SetState<MobRoam>();
                else
                    SetState<MobIdle>();
            }
        }
    }

    public void OnDrawGizmos()
    {
        if (Camera.current != Camera.main)
            return;

        GetModule<GroundPathingModule>().DrawGizmos();
    }
}