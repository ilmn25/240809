using UnityEngine;

/// <summary>A mischievous gnome that stalks the player from a distance. It sneaks
/// up and steals items lying on the ground nearby, stashing them in its pack. If
/// you kill it, you get your items back; if it flees and despawns, they're gone.</summary>
public class GnomeMachine : GroundMobMachine
{
    private const int FollowDistance = 6;
    private const int FleeDistance = 30;
    private const int RatCount = 1;

    private bool _fleeing;

    public static Info CreateInfo()
    {
        return new GnomeInfo()
        {
            HealthMax = 5,
            DistAttack = 1,
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
        _fleeing = false;

        AddState(new MobIdle());
        AddState(new MobStalk(FollowDistance));
        AddState(new MobStealItem());
        AddState(new MobRoam());
        AddState(new MobFleeDespawn(FleeDistance));
        AddState(new MobHit());
        AddState(new EquipSelectState());

        for (int i = 0; i < RatCount; i++)
        {
            Info ratInfo = Entity.Spawn(ID.Rat, Vector3Int.FloorToInt(transform.position));
            if (ratInfo?.Machine is RatMachine rat)
                rat.Gnome = this;
        }
    }

    public override void OnUpdate()
    {
        if (IsCurrentState<MobFleeDespawn>())
            return;

        // DroppedItem always points at the latest item a player dropped; it's only
        // ever stale once that item is gone, so reading it fresh each frame avoids
        // caching (and chasing) a destroyed reference forever.
        ItemInfo dropped = FindDroppedItem();
        if (dropped != null)
        {
            Info.Target = dropped;
            Info.PathingStatus = PathingStatus.Pending;
            SetState<MobStealItem>();
            return;
        }

        if (!IsCurrentState<DefaultState>())
            return;

        // Only stalk/steal from a player if they're alive and holding an item.
        PlayerInfo player = EntityScan.FindNearestPlayer(transform.position, Info.DistAlert, p => p.CanBeRobbed);

        if (player != null && Vector3.Distance(player.position, transform.position) > FollowDistance)
        {
            Info.Target = player;
            Info.PathingStatus = PathingStatus.Pending;
            SetState<MobStalk>();
        }
        else 
            SetState<MobRoam>();
    }

    private static ItemInfo FindDroppedItem()
    {
        if (Save.Inst?.players == null) return null;
        foreach (PlayerInfo player in Save.Inst.players)
        {
            if (player == null || player.Destroyed) continue;
            if (player.DroppedItem is { Destroyed: false } item) return item;
        }
        return null;
    }

    private void Steal(ItemInfo item)
    {
        if (Info is not GnomeInfo gnome || gnome.Stolen == null) return;
        Info.SetEquipment(new ItemSlot { ID = item.item.ID, Stack = item.item.Stack });
        gnome.Stolen.CreateAndAddItem(item.item.ID, item.item.Stack);
        item.Destroy();
    }

    /// <summary>Steal the currently targeted dropped item and flee. Called by
    /// MobStealItem once the gnome reaches the item.</summary>
    public void GrabAndFlee()
    {
        if (Info.Target is not ItemInfo item || item.Destroyed) return;
        Steal(item);
        StartFlee();
    }

    public void StartFlee()
    {
        if (_fleeing) return;
        _fleeing = true;

        Info.Target = EntityScan.FindNearestPlayer(transform.position, FleeDistance);
        Info.PathingStatus = PathingStatus.Pending;
        SetState<MobFleeDespawn>();
    }
}
