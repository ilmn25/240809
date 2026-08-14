using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>A mischievous gnome that stalks the player from a distance. It sneaks
/// up and steals items lying on the ground nearby, stashing them in its pack. If
/// you kill it, you get your items back; if it flees and despawns, they're gone.</summary>
public class GnomeMachine : GroundMobMachine
{
    private const int FollowDistance = 6;   // how close it creeps before stopping
    private const int FleeDistance = 30;    // how far it flees before despawning
    private const float PickupRadius = 3f;  // how close an item must be to steal
    private const int PickupInterval = 40;  // frames between pickup checks
    private const int RatCount = 2;         // how many rats accompany the gnome

    private int _pickupTimer;
    private bool _fleeing;

    public static Info CreateInfo()
    {
        return new GnomeInfo()
        {
            HealthMax = 20,
            DistAlert = 16,
            DistDisengage = FleeDistance,
            DistEscape = FleeDistance,
            DistRoam = 4,
            SpeedGround = 7,
            SpeedAir = 8,
            PathAir = 3,
        };
    }

    public override void OnStart()
    {
        base.OnStart();

        AddState(new MobIdle());
        AddState(new MobStalk(FollowDistance));
        AddState(new MobRoam());
        AddState(new MobFleeDespawn(FleeDistance));
        AddState(new MobHit());
        AddState(new EquipSelectState());

        // Spawn a small pack of rats to work with the gnome.
        for (int i = 0; i < RatCount; i++)
        {
            Vector3Int spawnPos = Vector3Int.FloorToInt(transform.position) + new Vector3Int(i + 1, 1, 0);
            Entity.Spawn(ID.Rat, spawnPos);
        }
    }

    public override void OnUpdate()
    {
        if (IsCurrentState<MobFleeDespawn>())
            return;

        // Steal items on the ground nearby.
        if (++_pickupTimer >= PickupInterval)
        {
            _pickupTimer = 0;
            if (StealNearbyItems())
            {
                // Got something — the whole group escapes.
                StartFlee();
                return;
            }
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

    /// <summary>Steal any items on the ground within pickup radius. Returns true
    /// if at least one item was stolen.</summary>
    private bool StealNearbyItems()
    {
        if (Info is not GnomeInfo gnome || gnome.Stolen == null) return false;

        bool stole = false;
        foreach (var em in new System.Collections.Generic.List<EntityMachine>(EntityItemLoad.ActiveEntities))
        {
            if (em == null || em.Info is not ItemInfo item || item.Destroyed) continue;
            if (Vector3.Distance(item.position, transform.position) > PickupRadius) continue;

            // Steal the item into the gnome's pack and remove it from the ground.
            gnome.Stolen.CreateAndAddItem(item.item.ID, item.item.Stack);
            item.Destroy();
            stole = true;
        }
        return stole;
    }

    /// <summary>Begin fleeing away from the player; the whole group (gnome + rats)
    /// escapes, and the gnome despawns once far enough.</summary>
    public void StartFlee()
    {
        if (_fleeing) return;
        _fleeing = true;

        // Tell nearby rats to flee too.
        foreach (var em in EntityDynamicLoad.ActiveEntities)
        {
            if (em != null && em is RatMachine rat)
                rat.StartFlee();
        }

        Info.Target = Main.PlayerInfo;
        Info.PathingStatus = PathingStatus.Pending;
        SetState<MobFleeDespawn>();
    }
}
