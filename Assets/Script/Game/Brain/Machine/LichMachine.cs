using UnityEngine;

/// <summary>Lich enemy: agonizingly slow but hits hard and is very tanky.
/// Relentlessly closes on the player — get away or get crushed. Only stalks
/// during the Rapture and vanishes when it ends.</summary>
public class LichMachine : MobMachine
{
    private static readonly ProjectileInfo CrushProjectile = new ContactDamageProjectileInfo {
        Damage = 10,
        Knockback = 30,
        CritChance = 0.1f,
        Radius = 1.2f,
    };

    public static Info CreateInfo()
    {
        return new EnemyInfo()
        {
            HealthMax = 200,
            DistAttack = 2,
            DistAlert = 30,
            DistDisengage = 30,
            DistRoam = 5,
            SpeedGround = 0.8f,
            SpeedAir = 1.0f,
            SpeedLogic = 0.8f,
            PathJump = 1,
            PathAir = 3,
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
        AddState(new MobHit());
        AddState(new MobAttackStopSwing(CrushProjectile));
        AddState(new EquipSelectState());
    }

    public override void OnUpdate()
    {
        // The Rapture ends — vanish in a puff.
        if (Save.Inst.weather != EnvironmentType.Rapture)
        {
            if (!Info.Destroyed)
            {
                Particle.Create(transform.position, Particles.HitDust, true);
                Info.Destroy();
            }
            return;
        }

        bool playerAlive = Main.PlayerInfo != null && !Main.PlayerInfo.Destroyed;
        bool playerInRange = playerAlive &&
            Vector3.Distance(Main.PlayerInfo.position, transform.position) < Info.DistAlert;

        // Relentless: once it sees you, it never gives up. It locks onto the player
        // from afar and only releases if they escape the (generous) disengage range.
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
            else if (Random.value > 0.5f)
                SetState<MobRoam>();
            else
                SetState<MobIdle>();
        }
    }

    public void OnDrawGizmos()
    {
        if (Camera.current != Camera.main)
            return;

        GetModule<GroundPathingModule>().DrawGizmos();
    }
}