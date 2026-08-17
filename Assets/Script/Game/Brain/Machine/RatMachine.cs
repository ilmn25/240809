using UnityEngine;

/// <summary>A rat that works with the gnome. It bites the player to damage them
/// (which knocks their held item to the ground), then the gnome grabs the dropped
/// item and both escape.</summary>
public class RatMachine : GroundMobMachine, IItemThief
{
    private const int FleeDistance = 30;

    private static readonly ProjectileInfo BiteProjectile = new ContactDamageProjectileInfo {
        Damage = 4,
        Knockback = 6,
        CritChance = 0.1f,
        Radius = 0.8f,
    };

    public static Info CreateInfo()
    {
        return new EnemyInfo()
        {
            HealthMax = 10,
            DistAttack = 2,
            DistAlert = 12,
            DistDisengage = FleeDistance,
            DistEscape = FleeDistance,
            DistRoam = 4,
            SpeedGround = 9,
            SpeedAir = 10,
            PathAir = 3,
        };
    }

    public override void OnStart()
    {
        base.OnStart();

        AddState(new MobIdle());
        AddState(new MobChase());
        AddState(new MobRoam());
        AddState(new MobFleeDespawn(FleeDistance));
        AddState(new MobHit());
        AddState(new MobAttackStopSwing(BiteProjectile));
        AddState(new EquipSelectState());
    }

    public override void OnUpdate()
    {
        if (IsCurrentState<MobFleeDespawn>())
            return;

        if (!IsCurrentState<DefaultState>())
            return;

        PlayerInfo player = Main.PlayerInfo;
        if (player == null || !player.CanBeRobbed)
        {
            SetState<MobRoam>();
            return;
        }

        float dist = Vector3.Distance(player.position, transform.position);

        if (dist < Info.DistAttack)
        {
            Info.AimPosition = Main.PlayerInfo.position;
            SetState<MobAttackStopSwing>();
            return;
        }

        if (dist < Info.DistAlert)
        {
            Info.Target = Main.PlayerInfo;
            Info.PathingStatus = PathingStatus.Pending;
            SetState<MobChase>();
        }
        else 
            SetState<MobRoam>();
    }

    public void StartFlee()
    {
        Info.Target = Main.PlayerInfo;
        Info.PathingStatus = PathingStatus.Pending;
        SetState<MobFleeDespawn>();
    }
}
