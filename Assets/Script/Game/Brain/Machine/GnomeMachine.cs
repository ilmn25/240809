using UnityEngine;

/// <summary>A mischievous gnome that stalks the player from a distance. It sneaks
/// up and steals items lying on the ground nearby, stashing them in its pack. If
/// you kill it, you get your items back; if it flees and despawns, they're gone.</summary>
public class GnomeMachine : GroundMobMachine
{
    private const int FollowDistance = 6;   // how close it creeps before stopping
    private const int FleeDistance = 30;    // how far it flees before despawning
    private const float PickupRadius = 1f;  // how close an item must be to steal
    private const int RatCount = 1;         // how many rats accompany the gnome

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
        AddState(new MobChase());
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

        // The player dropped an item (from a rat bite) — go pick it up and run.
        if (Main.PlayerInfo is { Destroyed: false } && Main.PlayerInfo.DroppedItem != null)
        {
            ItemInfo item = Main.PlayerInfo.DroppedItem;
            if (item.Destroyed)
            {
                // It was picked back up or vanished — forget it and stop chasing.
                Main.PlayerInfo.DroppedItem = null;
                Info.CancelTarget();
            }
            else if (Vector3.Distance(item.position, transform.position) <= PickupRadius)
            {
                Steal(item);
                Main.PlayerInfo.DroppedItem = null;
                StartFlee();
                return;
            }
            else
            {
                // Not close enough yet — chase the item directly.
                Info.Target = item;
                Info.PathingStatus = PathingStatus.Pending;
                SetState<MobChase>();
                return;
            }
        }

        if (!IsCurrentState<DefaultState>())
            return;

        // Only stalk/steal from the player if they're alive and holding an item.
        PlayerInfo player = Main.PlayerInfo;
        float playerDist = player != null && player.CanBeRobbed
            ? Vector3.Distance(player.position, transform.position)
            : float.MaxValue;

        if (playerDist < Info.DistAlert && playerDist > FollowDistance)
        {
            Info.Target = Main.PlayerInfo;
            Info.PathingStatus = PathingStatus.Pending;
            SetState<MobStalk>();
        }
        else 
            SetState<MobRoam>();
    }

    /// <summary>Take the item — hold it in hand, stash it, and remove it from the ground.</summary>
    private void Steal(ItemInfo item)
    {
        if (Info is not GnomeInfo gnome || gnome.Stolen == null) return;
        Info.SetEquipment(new ItemSlot { ID = item.item.ID, Stack = item.item.Stack });
        gnome.Stolen.CreateAndAddItem(item.item.ID, item.item.Stack);
        item.Destroy();
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
            {
                rat.StartFlee();
                return;
            } 
        }

        Info.Target = Main.PlayerInfo;
        Info.PathingStatus = PathingStatus.Pending;
        SetState<MobFleeDespawn>();
    }
}
