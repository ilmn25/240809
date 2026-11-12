using UnityEngine;

/// <summary>A skittish crow, in the spirit of Don't Starve's birds: it pecks around
/// the ground for a while, then bolts into the air and flies off — immediately if the
/// player comes close.</summary>
public class CrowMachine : GroundMobMachine
{
    private const float ScareDistance = 6f;     // player this close sends it flying
    private const float FlightHeight = 7f;      // how high it climbs on take-off
    private const float FleeDistance = 30f;     // how far it flies before despawning
    private const int GroundTimeMin = 60 * 20;  // pecks around for 20-40s before leaving
    private const int GroundTimeMax = 60 * 40;
    private const int LandingTimeout = 60 * 10; // gives up if it can't reach the ground

    private int _groundTimer;
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
            DistRoam = 4,
        };
    }

    public override void OnStart()
    {
        base.OnStart();

        AddState(new MobIdle());
        AddState(new MobRoam());
        AddState(new MobTakeOff(FlightHeight, FleeDistance));

        _fleeing = false;
        _airborneTimer = 0;
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

        if (Random.value > 0.5f) SetState<MobRoam>();
        else SetState<MobIdle>();
    }

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
