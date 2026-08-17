using UnityEngine;

/// <summary>A rat that works with the gnome. It bites the player to damage them
/// (which knocks their held item to the ground), then the gnome grabs the dropped
/// item and both escape.</summary>
public class RatMachine : GroundMobMachine, IItemThief
{
    private const int FleeDistance = 30;

    /// <summary>The gnome this rat follows when it has no player target, and
    /// escapes with when the gnome flees.</summary>
    public GnomeMachine Gnome;

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
        AddState(new MobHit());
        AddState(new MobAttackStopSwing(BiteProjectile));
        AddState(new EquipSelectState());
    }

    public override void OnUpdate()
    {
        // The gnome vanished, so this rat leaves too.
        if (Gnome == null || Gnome.Info == null || Gnome.Info.Destroyed)
        {
            Info.Destroy();
            return;
        }

        if (!IsCurrentState<DefaultState>())
            return;

        // While the gnome is fleeing, the whole pack escapes with it regardless of
        // the player's position — no explicit "call each rat to leave" is needed.
        if (Gnome.IsCurrentState<MobFleeDespawn>())
        {
            FollowGnome();
            return;
        }

        PlayerInfo player = Main.PlayerInfo;
        if (player == null || !player.CanBeRobbed)
        {
            FollowGnome();
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
            FollowGnome();
    }

    /// <summary>Chase the gnome and hover near it.</summary>
    private void FollowGnome()
    {
        if (Vector3.Distance(Gnome.transform.position, transform.position) <= Info.DistAttack)
            return;
        Info.Target = Gnome.Info;
        Info.PathingStatus = PathingStatus.Pending;
        SetState<MobChase>();
    }
}
