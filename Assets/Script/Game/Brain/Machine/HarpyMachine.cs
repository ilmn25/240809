 
using UnityEngine;
using Random = UnityEngine.Random;

public class HarpyMachine : GroundMobMachine
{
    protected override bool UsesDoorBash => true;

    private const int StalkDistance = 6;   // how close a lone harpy creeps before stopping
    private const int GroupAttackCount = 3; // harpies needed before they dive in
    private const float GroupRadius = 8f;   // how close harpies must be to count as grouped

    private bool _committed;               // latched once grouped — keeps them attacking

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
        AddState(new MobAttackSwing(TalonProjectile));
        AddState(new EquipSelectState());
    }

    public override void OnUpdate()
    {
        if (!IsCurrentState<DefaultState>()) return;

        // Aggressive: lock onto the player on sight, drop them once they flee.
        UpdatePlayerAggro();

        if (Info.Target == null)
        {
            _committed = false;
            RoamOrIdle();
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
                SetState<MobEvade>(); // too close while stalking: back off
            else if (Info.PathingStatus == PathingStatus.Stuck)
                SetState<MobRoam>();
            else
                SetState<MobStalk>();
            return;
        }

        // Committed — dive in and attack.
        EngageTarget();
    }

    /// <summary>Counts how many harpies are grouped near this one (including itself).</summary>
    private int GroupedHarpies()
    {
        return 1 + EntityScan.Count(transform.position, GroupRadius, i => i.Machine is HarpyMachine h && h != this);
    }

} 