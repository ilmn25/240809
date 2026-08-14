using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>A thrall — a reanimated ghoul-like minion summoned by a cultist. It
/// chases the player and freezes in place to swing when in melee range (the
/// stop-swing behavior shared by ghouls and spiders).</summary>
public class ThrallMachine : GroundMobMachine
{
    protected override bool UsesDoorBash => true;

    private static readonly ProjectileInfo ClawProjectile = new ContactDamageProjectileInfo {
        Damage = 1,          // low attack — they swarm in numbers
        Knockback = 6,
        CritChance = 0.05f,
        Radius = 0.7f,
    };

    public static Info CreateInfo()
    {
        return new EnemyInfo()
        {
            HealthMax = 8,    // low HP — they swarm in numbers
            DistAlert = 14,
            DistDisengage = 18,
            DistRoam = 5,
            SpeedGround = 4,
            SpeedAir = 5,
            PathJump = 1,
            PathAir = 3,
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
        AddState(new MobAttackStopSwing(ClawProjectile));
        AddState(new EquipSelectState());
    }

    public override void OnUpdate()
    {
        UpdateAggro();

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
}
