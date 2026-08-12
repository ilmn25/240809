 
using UnityEngine;
using Random = UnityEngine.Random;

public class HarpyMachine : GroundMobMachine
{
    protected override bool UsesDoorBash => true;

    private const int StalkDistance = 6;   // how close a lone harpy creeps before stopping
    private const int GroupAttackCount = 3; // harpies needed before they dive in
    private const float GroupRadius = 8f;   // how close harpies must be to count as grouped

    private bool _committed;               // latched once grouped — keeps them attacking

    private static readonly Collider[] HarpyScanBuffer = new Collider[16];
    private static readonly ProjectileInfo TalonProjectile = new ContactDamageProjectileInfo {
        Damage = 3,
        Knockback = 9,
        CritChance = 0.1f,
        Radius = 0.8f,
    };

    public static Info CreateInfo()
    {
        return new EnemyInfo()
        {
            HealthMax = 16,
            DistAttack = 2,
            DistAlert = 16,
            DistDisengage = 26,
            DistRoam = 7,
            PathJump = 10,
            PathAir = -1,
            SpeedGround = 6,
            SpeedAir = 7,
            JumpVelocity = 7,
            CanFly = true,
        };
    }

    public override void OnStart()
    {
        base.OnStart();

        AddState(new MobIdle());
        AddState(new MobChase());
        AddState(new MobStalk(StalkDistance));
        AddState(new MobRoam());
        AddState(new MobEvade());
        AddState(new MobHit());
        AddState(new MobAttackStopSwing(TalonProjectile));
        AddState(new EquipSelectState());
    }

    public override void OnUpdate()
    {
        if (IsCurrentState<DefaultState>())
        {
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
                _committed = false;
                if (Random.value > 0.5f)
                    SetState<MobRoam>();
                else
                    SetState<MobIdle>();
                return;
            }

            float dist = Vector3.Distance(Info.Target.position, transform.position);

            // Latch onto the attack once enough harpies group up, so a few of them
            // spreading out mid-dive doesn't make the group re-stalk.
            if (!_committed && GroupedHarpies() >= GroupAttackCount)
                _committed = true;

            // Not committed yet — stalk from a distance.
            if (!_committed)
            {
                if (dist < StalkDistance)
                {
                    // Too close while stalking: back off to keep the stalk distance.
                    SetState<MobEvade>();
                }
                else if (Info.PathingStatus == PathingStatus.Stuck)
                {
                    SetState<MobRoam>();
                }
                else
                {
                    SetState<MobStalk>();
                }
                return;
            }

            // Committed — dive in and attack.
            if (dist < Info.DistAttack)
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
    }

    /// <summary>Counts how many harpies are grouped near this one (including itself).</summary>
    private int GroupedHarpies()
    {
        int count = 1;
        int hits = Physics.OverlapSphereNonAlloc(transform.position, GroupRadius, HarpyScanBuffer, Main.MaskEntity);
        for (int i = 0; i < hits; i++)
        {
            if (HarpyScanBuffer[i].TryGetComponent(out HarpyMachine other) && other != this)
                count++;
        }
        return count;
    }

} 