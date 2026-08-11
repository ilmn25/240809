using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>A tree that's secretly alive. At night it creeps toward the player and
/// follows from a distance. If the player attacks it, it drops a little loot and
/// flees, despawning once it gets away. It also flees and despawns at dawn.
/// Killing it outright drops more loot.</summary>
public class TreeMimicMachine : MobMachine
{
    private const int FollowDistance = 6;   // how close it creeps before stopping
    private const int FleeDistance = 30;     // how far it flees before despawning

    public static Info CreateInfo()
    {
        return new TreeMimicInfo()
        {
            HealthMax = 30,
            DistAttack = 2,
            DistAlert = 16,            // how far away it notices and starts following
            DistDisengage = FleeDistance,
            DistEscape = FleeDistance,
            DistRoam = 4,
            SpeedGround = 7,
            SpeedAir = 8,
            PathJump = 1,
            PathAir = 3,
        };
    }

    public override void OnStart()
    {
        AddModule(new GroundMovementModule());
        AddModule(new GroundPathingModule());
        AddModule(new GroundAnimationModule());
        AddModule(new MobSpriteCullModule());
        AddModule(new SpriteOrbitModule());

        AddState(new MobIdle());
        AddState(new MobStalk(FollowDistance));
        AddState(new MobRoam());
        AddState(new MobFleeDespawn(FleeDistance));
        AddState(new MobHit());
        AddState(new EquipSelectState());
    }

    public override void OnUpdate()
    {
        if (IsCurrentState<MobFleeDespawn>())
            return;

        bool night = Save.Inst.weather == EnvironmentType.NightRainy ||
                     Save.Inst.weather == EnvironmentType.NightBright;

        // Day breaks — run away and despawn instead of vanishing in place.
        if (!night)
        {
            StartFlee();
            return;
        }

        if (!IsCurrentState<DefaultState>())
            return;

        bool playerAlive = Main.PlayerInfo != null && !Main.PlayerInfo.Destroyed;
        float playerDist = playerAlive
            ? Vector3.Distance(Main.PlayerInfo.position, transform.position)
            : float.MaxValue;

        if (playerAlive && playerDist < Info.DistAlert)
        {
            if (playerDist > FollowDistance)
            {
                Info.Target = Main.PlayerInfo;
                Info.PathingStatus = PathingStatus.Pending;
                SetState<MobStalk>();
            }
            else
            {
                Info.Target = null;
                Info.PathingStatus = PathingStatus.Stuck;
                SetState<MobIdle>();
            }
        }
        else if (Random.value > 0.5f)
            SetState<MobRoam>();
        else
            SetState<MobIdle>();
    }

    public void StartFlee()
    {
        Info.Target = Main.PlayerInfo;
        Info.PathingStatus = PathingStatus.Pending;
        SetState<MobFleeDespawn>();
    }

    public void OnDrawGizmos()
    {
        if (Camera.current != Camera.main)
            return;

        GetModule<GroundPathingModule>().DrawGizmos();
    }
}
