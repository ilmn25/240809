using UnityEngine;

/// <summary>A pigeon that flies in from a distance, hovers high above the player,
/// drops a poop projectile straight down on them, then flies away and despawns.
/// It uses HoverFlightModule to stay airborne at all times (immune to gravity and
/// the floor), and MobFleeDespawn to fly away once it's done.</summary>
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
    private const int FleeDistance = 30;     // how far it flees before despawning

    private HoverFlightModule _hover;
    private bool _leaving;

    /// <summary>True once the pigeon has started flying away.</summary>
    public bool Leaving => _leaving;

    public static Info CreateInfo()
    {
        return new PigeonInfo()
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

        _hover = AddModule(new HoverFlightModule {
            HoverHeight = HoverHeight,
        });

        AddState(new MobFleeDespawn(FleeDistance));
        AddState(new EquipSelectState());

        // Spawn point is on the ground — lift it to hover height immediately so it
        // is airborne (and visibly flying in) from the very first frame.
        Vector3 pos = transform.position;
        pos.y += HoverHeight;
        transform.position = pos;
    }

    public override void OnUpdate()
    {
        if (_leaving)
            return;

        // Hover over the player (the module flies us in and holds altitude), then
        // poop the moment we're overhead and leave immediately.
        if (Main.PlayerInfo != null && !Main.PlayerInfo.Destroyed)
        {
            Info.Target = Main.PlayerInfo;
            if (_hover.IsOverhead)
                DropPoop();
        }
        else
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

    /// <summary>Begin fleeing away from the player; despawns once far enough. Also
    /// called by PigeonInfo when the pigeon is hit mid-escape so it stays leaving.</summary>
    public void StartFlee()
    {
        _leaving = true;
        SetState<MobFleeDespawn>();
    }
}
