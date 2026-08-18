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

    private const float HoverHeight = 6f;   // how high above the player it hovers
    private const int FleeDistance = 30;     // how far it flees before despawning
    private const float ScarecrowRadius = 12f; // radius around a scarecrow pigeons avoid
    private const int DepartDelay = 30;      // frames (~0.5s) before flying off after the drop

    private static readonly Collider[] ScarecrowScan = new Collider[16];

    private HoverFlightModule _hover;
    private bool _leaving;
    private Vector3 _approachDir; // horizontal direction flown in — keeps moving during the linger
    private int _departTimer;

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
        // Linger: glide forward a moment after the drop, then leave.
        if (_departTimer > 0)
        {
            Info.Direction.x = _approachDir.x;
            Info.Direction.z = _approachDir.z;
            if (--_departTimer == 0)
                StartFlee();
            return;
        }

        if (_leaving)
            return;

        if (Main.PlayerInfo == null || Main.PlayerInfo.Destroyed || ScarecrowNear(Main.PlayerInfo.position))
        {
            StartFlee();
            return;
        }

        // Keep the direction we're flying so we glide forward during the linger.
        if (Info.Direction.sqrMagnitude > 0.001f)
            _approachDir = Info.Direction;

        Info.Target = Main.PlayerInfo;
        if (_hover.IsOverhead)
            DropPoop();
    }

    private static bool ScarecrowNear(Vector3 pos)
    {
        int count = Physics.OverlapSphereNonAlloc(pos, ScarecrowRadius, ScarecrowScan, Main.MaskEntity);
        for (int i = 0; i < count; i++)
            if (ScarecrowScan[i].TryGetComponent<ScarecrowMachine>(out _))
                return true;
        return false;
    }

    /// <summary>Drop a poop projectile straight down, then keep flying forward for a
    /// beat before flying off.</summary>
    private void DropPoop()
    {
        Vector3 origin = transform.position;
        Vector3 dest = origin + Vector3.down * HoverHeight;
        Projectile.Spawn(origin, dest, PoopProjectile, HitboxType.Player, Info);
        Audio.PlaySFX(SfxID.HitStone);
        // Mark leaving so HoverFlightModule keeps it airborne (only controls Y) and
        // doesn't pull it back toward the player while it lingers.
        _leaving = true;
        _departTimer = DepartDelay;
    }

    /// <summary>Begin fleeing away from the player; despawns once far enough. Also
    /// called by PigeonInfo when the pigeon is hit mid-escape so it stays leaving.</summary>
    public void StartFlee()
    {
        _leaving = true;
        SetState<MobFleeDespawn>();
    }
}
