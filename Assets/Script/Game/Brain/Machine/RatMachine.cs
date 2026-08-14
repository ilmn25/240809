using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>A rat that works with the gnome. It bites the player to make them drop
/// their held item, then flees. The gnome picks up the dropped item and both escape.</summary>
public class RatMachine : GroundMobMachine
{
    private const int BiteCooldown = 120;   // frames between bites
    private const int FleeDistance = 30;    // how far it flees before despawning

    private int _biteTimer;

    public static Info CreateInfo()
    {
        return new EnemyInfo()
        {
            HealthMax = 10,
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
        AddState(new EquipSelectState());
    }

    public override void OnUpdate()
    {
        if (IsCurrentState<MobFleeDespawn>())
            return;

        if (!IsCurrentState<DefaultState>())
            return;

        bool playerAlive = Main.PlayerInfo != null && !Main.PlayerInfo.Destroyed;
        if (!playerAlive)
        {
            if (Random.value > 0.5f) SetState<MobRoam>();
            else SetState<MobIdle>();
            return;
        }

        float dist = Vector3.Distance(Main.PlayerInfo.position, transform.position);

        // Bite the player to make them drop their held item.
        if (dist < Info.DistAttack && ++_biteTimer >= BiteCooldown)
        {
            _biteTimer = 0;
            if (ForceDropHeldItem())
                StartFlee(); // got the item — run away
            return;
        }

        if (dist < Info.DistAlert)
        {
            Info.Target = Main.PlayerInfo;
            Info.PathingStatus = PathingStatus.Pending;
            SetState<MobChase>();
        }
        else if (Random.value > 0.5f)
            SetState<MobRoam>();
        else
            SetState<MobIdle>();
    }

    /// <summary>Force the player to drop their held item onto the ground. Returns
    /// true if an item was dropped.</summary>
    private bool ForceDropHeldItem()
    {
        PlayerInfo player = Main.PlayerInfo;
        Storage storage = player.Storage;
        if (storage?.List == null || storage.Key < 0 || storage.Key >= storage.List.Count) return false;

        ItemSlot held = storage.List[storage.Key];
        if (held == null || held.isEmpty()) return false;

        // Spawn the held item on the ground for the gnome to grab, then clear the slot.
        Entity.SpawnItem(held, player.position, stackOnSpawn: false);
        held.clear();
        storage.NotifyChanged();
        return true;
    }

    /// <summary>Begin fleeing away from the player; despawns once far enough.</summary>
    public void StartFlee()
    {
        Info.Target = Main.PlayerInfo;
        Info.PathingStatus = PathingStatus.Pending;
        SetState<MobFleeDespawn>();
    }
}
