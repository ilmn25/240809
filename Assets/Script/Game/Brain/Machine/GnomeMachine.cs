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
            HealthMax = 20,
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
            Vector3Int spawnPos = Vector3Int.FloorToInt(transform.position) + new Vector3Int(i + 1, 1, 0);
            Info ratInfo = Entity.Spawn(ID.Rat, spawnPos);
            if (ratInfo?.Machine is RatMachine rat)
                rat.Gnome = this;
        }
    }

    public override void OnUpdate()
    {
        if (IsCurrentState<MobFleeDespawn>())
            return;

        // DroppedItem always points at the latest item the player dropped; it's only
        // ever stale once that item is gone, so reading it fresh each frame avoids
        // caching (and chasing) a destroyed reference forever.
        ItemInfo dropped = Main.PlayerInfo?.DroppedItem;
        if (dropped != null && !dropped.Destroyed)
        {
            Info.Target = dropped;
            Info.PathingStatus = PathingStatus.Pending;
            SetState<MobStealItem>();
            return;
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

        Info.Target = Main.PlayerInfo;
        Info.PathingStatus = PathingStatus.Pending;
        SetState<MobFleeDespawn>();
    }
}
