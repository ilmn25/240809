using UnityEngine;

/// <summary>A heavy brick dungeon door set into the wall of a stairwell entrance.
/// Right-clicking it sends the player to the dungeon world.</summary>
public class DungeonDoorMachine : StructureMachine, IActionSecondaryInteract
{
    private const int DoorHealth = 1000;

    public static Info CreateInfo()
    {
        return new StructureInfo
        {
            Health = DoorHealth,
            threshold = 1,
            operationType = OperationType.Cutting,
            Loot = ID.Null,
            SfxHit = SfxID.HitStone,
            SfxDestroy = SfxID.HitStone,
            SpawnsRubble = false,
        };
    }

    public void OnActionSecondary(Info info)
    {
        if (info is not PlayerInfo) return; // only players can use the door
        Scene.SwitchWorld(GenType.Dungeon);
    }
}
