using UnityEngine;

/// <summary>A heavy brick dungeon door set into the wall of a stairwell entrance.
/// Right-clicking it sends the player to the dungeon world.</summary>
public class DungeonDoorMachine : StructureMachine, IActionSecondaryInteract
{
    private const int DoorHealth = 1000;

    /// <summary>How near the floor's spawn a door must sit to be the way back up: the up
    /// stairwell's doorway is pasted on the spawn point itself, while the stairwell down is
    /// always placed far away from it (see GenDungeon).</summary>
    private const int UpDoorRadius = 24;

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
            GlowOn = true,
        };
    }

    public void OnActionSecondary(Info info)
    {
        if (info is not PlayerInfo) return; // only players can use the door

        if (Save.Inst.current != GenType.Dungeon)
        {
            // The surface entrance always starts a brand new run on the first floor.
            Save.Inst.dungeonDepth = 1;
            Scene.SwitchWorld(GenType.Dungeon);
            return;
        }

        bool up = IsAtSpawn(info.position);
        if (up && Save.Inst.DungeonDepth <= 1)
        {
            Save.Inst.dungeonDepth = 1; // leaving the dungeon ends the run
            Scene.SwitchWorld(GenType.Abyss);
            return;
        }

        // Every floor is generated on arrival, so climbing as well as descending leads
        // into a floor nobody has seen before.
        Save.Inst.dungeonDepth = Mathf.Max(1, Save.Inst.DungeonDepth + (up ? -1 : 1));
        Scene.SwitchWorld(GenType.Dungeon);
    }

    /// <summary>True when this door is the stairwell the players spawned on.</summary>
    private static bool IsAtSpawn(Vector3 position)
    {
        Vector3Int spawn = World.Inst.SpawnPoint;
        float dx = position.x - spawn.x;
        float dz = position.z - spawn.z;
        return dx * dx + dz * dz <= UpDoorRadius * UpDoorRadius;
    }
}
