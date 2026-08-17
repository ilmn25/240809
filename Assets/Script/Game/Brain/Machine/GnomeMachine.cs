using System.Collections.Generic;
using UnityEngine;

/// <summary>A mischievous gnome that stalks the player from a distance. It sneaks
/// up and steals items lying on the ground nearby, stashing them in its pack. If
/// you kill it, you get your items back; if it flees and despawns, they're gone.</summary>
public class GnomeMachine : GroundMobMachine
{
    private const int FollowDistance = 6;
    private const int FleeDistance = 30;
    private const float PickupRadius = 1.2f;
    private const int RatCount = 1;

    private readonly List<RatMachine> _rats = new();
    private ItemInfo _targetItem;
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
        _rats.Clear();
        _targetItem = null;
        _fleeing = false;

        AddState(new MobIdle());
        AddState(new MobStalk(FollowDistance));
        AddState(new MobChase());
        AddState(new MobRoam());
        AddState(new MobFleeDespawn(FleeDistance));
        AddState(new MobHit());
        AddState(new EquipSelectState());

        for (int i = 0; i < RatCount; i++)
        {
            Vector3Int spawnPos = Vector3Int.FloorToInt(transform.position) + new Vector3Int(i + 1, 1, 0);
            Info ratInfo = Entity.Spawn(ID.Rat, spawnPos);
            if (ratInfo?.Machine is RatMachine rat)
                _rats.Add(rat);
        }
    }

    public override void OnUpdate()
    {
        if (IsCurrentState<MobFleeDespawn>())
            return;

        if (_targetItem == null && Main.PlayerInfo != null)
            _targetItem = Main.PlayerInfo.DroppedItem;

        if (_targetItem != null)
        {
            if (_targetItem.Destroyed)
            {
                _targetItem = null;
                Info.CancelTarget();
            }
            else if (Vector3.Distance(_targetItem.position, transform.position) <= PickupRadius)
            {
                Steal(_targetItem);
                _targetItem = null;
                StartFlee();
                return;
            }
            else
            {
                Info.Target = _targetItem;
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

    private void Steal(ItemInfo item)
    {
        if (Info is not GnomeInfo gnome || gnome.Stolen == null) return;
        Info.SetEquipment(new ItemSlot { ID = item.item.ID, Stack = item.item.Stack });
        gnome.Stolen.CreateAndAddItem(item.item.ID, item.item.Stack);
        item.Destroy();
    }

    public void StartFlee()
    {
        if (_fleeing) return;
        _fleeing = true;

        foreach (RatMachine rat in _rats)
            rat.StartFlee();

        Info.Target = Main.PlayerInfo;
        Info.PathingStatus = PathingStatus.Pending;
        SetState<MobFleeDespawn>();
    }
}
