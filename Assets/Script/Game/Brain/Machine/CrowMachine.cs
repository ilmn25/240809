using UnityEngine;

/// <summary>A skittish crow, in the spirit of Don't Starve's birds: it pecks around
/// the ground for a while (helping itself to any seed lying there), then bolts into
/// the air and flies off — immediately if the player comes close.</summary>
public class CrowMachine : GroundMobMachine
{
    private const float ScareDistance = 6f;     // player this close sends it flying
    private const float FlightHeight = 7f;      // how high it climbs on take-off
    private const float FleeDistance = 30f;     // how far it flies before despawning
    private const float SeedSearchRadius = 6f;  // how far it looks for dropped seeds
    private const int SeedScanInterval = 20;    // frames between seed scans
    private const int GroundTimeMin = 60 * 20;  // pecks around for 20-40s before leaving
    private const int GroundTimeMax = 60 * 40;
    private const int LandingTimeout = 60 * 10; // gives up if it can't reach the ground

    private int _groundTimer;
    private int _seedScanTimer;
    private int _airborneTimer;
    private bool _fleeing;

    public static Info CreateInfo()
    {
        return new CrowInfo
        {
            HealthMax = 5,      // small and fragile
            Gravity = -22f,     // light, so hops and the take-off arc feel floaty
            JumpVelocity = 8f,  // steady wing beats when taking off
            SpeedGround = 4,
            SpeedAir = 9,
            PathAir = 3,
            DistAttack = 1,     // how close it gets to a seed before pecking it up
            DistRoam = 4,
        };
    }

    public override void OnStart()
    {
        base.OnStart();

        AddState(new MobIdle());
        AddState(new MobRoam());
        AddState(new MobChase());
        AddState(new MobTakeOff(FlightHeight, FleeDistance));

        _groundTimer = Random.Range(GroundTimeMin, GroundTimeMax);
    }

    public override void OnUpdate()
    {
        if (_fleeing) return;

        if (!Info.IsGrounded)
        {
            // Safety net: a crow that can never touch down (buried in terrain, or
            // frozen mid-air in creative mode) leaves instead of lingering forever.
            if (++_airborneTimer >= LandingTimeout)
                StartFlee();
            return;
        }
        _airborneTimer = 0;

        PlayerInfo player = Main.PlayerInfo;
        if (player != null && !player.Destroyed &&
            Vector3.Distance(player.position, transform.position) < ScareDistance)
        {
            StartFlee();
            return;
        }

        if (--_groundTimer <= 0)
        {
            StartFlee();
            return;
        }

        if (!IsCurrentState<DefaultState>()) return;

        if (TryEatSeed()) return;

        if (Random.value > 0.5f) SetState<MobRoam>();
        else SetState<MobIdle>();
    }

    /// <summary>Walk over to the nearest dropped seed and eat it. Returns true while a
    /// seed is being handled so the crow keeps pecking instead of wandering off.</summary>
    private bool TryEatSeed()
    {
        if (Info.Target is ItemInfo target && !target.Destroyed && IsSeed(target.item.ID))
        {
            if (Vector3.Distance(target.position, transform.position) <= Info.DistAttack)
            {
                target.Destroy();
                Info.CancelTarget();
                SetState<MobIdle>();
                return true;
            }

            if (Info.PathingStatus == PathingStatus.Stuck)
            {
                Info.CancelTarget();
                return false;
            }

            if (!IsCurrentState<MobChase>())
            {
                Info.PathingStatus = PathingStatus.Pending;
                SetState<MobChase>();
            }
            return true;
        }

        if (--_seedScanTimer > 0) return false;
        _seedScanTimer = SeedScanInterval;

        Info seed = EntityScan.FindNearest(transform.position, SeedSearchRadius,
            i => i is ItemInfo item && !item.Destroyed && IsSeed(item.item.ID));
        if (seed == null) return false;

        Info.Target = seed;
        Info.PathingStatus = PathingStatus.Pending;
        SetState<MobChase>();
        return true;
    }

    private static bool IsSeed(ID id) => id is ID.CornSeed or ID.PumpkinSeed or ID.Acorn;

    /// <summary>Take off and fly away from a threat (the player by default), then
    /// despawn. Also called by CrowInfo when the crow is struck.</summary>
    public void StartFlee(Info threat = null)
    {
        if (_fleeing) return;
        _fleeing = true;

        Info.Target = threat ?? Main.PlayerInfo;
        Info.PathingStatus = PathingStatus.Pending;
        SetState<MobTakeOff>();
    }
}
