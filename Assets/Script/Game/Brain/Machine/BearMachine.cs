using UnityEngine;

/// <summary>Bear enemy: aggressively chases its target down, then freezes in place
/// to swing when it closes to melee range (hound-in-Dont-Starve behavior).</summary>
public class BearMachine : MobMachine
{
    private static readonly ProjectileInfo ClawProjectile = new ContactDamageProjectileInfo {
        Damage = 3,
        Knockback = 14,
        CritChance = 0.1f,
        Radius = 0.9f,
    };

    public static Info CreateInfo()
    {
        return new EnemyInfo()
        {
            HealthMax = 42,
            DistAttack = 2,
            DistAlert = 16,
            DistDisengage = 18,
            DistRoam = 6,
            SpeedGround = 4,
            SpeedAir = 6,
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
        AddState(new MobRoam());
        AddState(new MobEvade());
        AddState(new MobHit());
        AddState(new MobAttackStopSwing(ClawProjectile));
        AddState(new EquipSelectState());
    }

    public override void OnUpdate()
    {
        bool playerAlive = Main.PlayerInfo != null && !Main.PlayerInfo.Destroyed;
        bool playerInRange = playerAlive &&
            Vector3.Distance(Main.PlayerInfo.position, transform.position) < Info.DistAlert;

        // Aggressive predator: mark the player as a target the moment they wander
        // into alert range, and let go once they retreat well out of reach.
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