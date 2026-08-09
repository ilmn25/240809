using UnityEngine;

/// <summary>A wooden door. A solid structure that blocks enemies (it occupies NavMap),
/// but players can right-click it to toggle it open/closed. Enemies can attack and
/// bash it down when it blocks their path.</summary>
public class DoorMachine : StructureMachine, IActionSecondaryInteract
{
    private const int DoorHealth = 150;
    private bool _open;

    public static Info CreateInfo()
    {
        return new StructureInfo
        {
            Health = DoorHealth,
            threshold = 1,
            operationType = OperationType.Cutting, // hit it with an axe, like other wooden structures
            Loot = ID.Door,
            EnemyBreakable = true,
            SfxHit = SfxID.HitStone,
            SfxDestroy = SfxID.HitStone,
        };
    }

    public void OnActionSecondary(Info info)
    {
        if (info is not PlayerInfo) return; // only players can open/close the door
        SetOpen(!_open);
    }

    private void SetOpen(bool open)
    {
        _open = open;
        SetBlocked(!open);
        SpriteRenderer.sprite = Cache.LoadSprite(open ? "Sprite/DoorOpen" : "Sprite/Door");
    }

    // Toggle the door's NavMap value: closed = Door (blocks movement, pathfinding
    // can still route through), open = Air (fully passable).
    private void SetBlocked(bool blocked)
    {
        byte value = blocked ? NavMap.Door : NavMap.Air;
        NavMap.SetEntity(Entity, transform.position, value);
        Vector3Int start = Vector3Int.FloorToInt(transform.position);
        Vector3Int size = Vector3Int.FloorToInt(Entity.Bounds);
        for (int x = 0; x < size.x; x++)
            for (int y = 0; y < size.y; y++)
                for (int z = 0; z < size.z; z++)
                    NavMapSync.BroadcastBlockUpdate(start + new Vector3Int(x, y, z), value);
    }
}
