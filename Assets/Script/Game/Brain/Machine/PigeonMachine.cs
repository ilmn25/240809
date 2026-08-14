using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>A pigeon that hovers high above the player, drops a poop projectile
/// straight down on them, then flies away and despawns. It uses HoverFlightModule
/// to stay at height (immune to gravity), so it never sits on the ground.</summary>
public class PigeonMachine : GroundMobMachine
{
    private static readonly PoopProjectileInfo PoopProjectile = new PoopProjectileInfo {
        Damage = 2,
        Knockback = 4,
        Radius = 0.6f,
        Speed = 10f,
        Sprite = ID.BirdShit,
        Scale = 0.6f,
    };

    private const float HoverHeight = 10f;   // how high above the player it hovers
    private const int PoopDelay = 120;       // frames before it poops (~2s)
    private const int FleeDistance = 30;     // how far it flees before despawning

    private int _timer;
    private bool _leaving;

    public static Info CreateInfo()
    {
        return new EnemyInfo()
        {
            HealthMax = 8,
            DistAlert = 20,
            DistDisengage = FleeDistance,
            DistEscape = FleeDistance,
            SpeedAir = 9,
            CanFly = true,
        };
    }

    public override void OnStart()
    {
        base.OnStart();

        AddModule(new HoverFlightModule { HoverHeight = HoverHeight });

        AddState(new MobIdle());
        AddState(new MobFleeDespawn(FleeDistance));
        AddState(new EquipSelectState());
    }

    public override void OnUpdate()
    {
        // Fleeing — let MobFleeDespawn fly us away and despawn off-screen.
        if (_leaving)
            return;

        // Hover over the player (the module holds altitude), then drop the poop.
        if (Main.PlayerInfo != null && !Main.PlayerInfo.Destroyed)
        {
            Info.Target = Main.PlayerInfo;
            if (++_timer >= PoopDelay)
                DropPoop();
        }
        else if (++_timer >= PoopDelay)
        {
            // No player — just leave.
            StartFlee();
        }
    }

    /// <summary>Drop a poop projectile straight down, then fly away.</summary>
    private void DropPoop()
    {
        Vector3 origin = transform.position;
        Vector3 dest = origin + Vector3.down * HoverHeight;
        Projectile.Spawn(origin, dest, PoopProjectile, HitboxType.Player, Info);
        Audio.PlaySFX(SfxID.HitStone);
        StartFlee();
    }

    /// <summary>Begin fleeing away from the player; despawns once far enough.</summary>
    private void StartFlee()
    {
        _leaving = true;
        SetState<MobFleeDespawn>();
    }
}
