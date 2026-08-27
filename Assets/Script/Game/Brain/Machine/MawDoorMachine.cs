using UnityEngine;

/// <summary>A heavy brick door set into the Maw's facility (and mirrored on the
/// Abyss surface as the way in). Right-clicking it travels between the Abyss and
/// the Maw's extraction facility.</summary>
public class MawDoorMachine : StructureMachine, IActionSecondaryInteract
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
            GlowOn = true,
        };
    }

    public override void OnSetup()
    {
        base.OnSetup();
        // Heavy facility door — reuse the dungeon-door sprite.
        SpriteRenderer.sprite = Cache.LoadSprite("Sprite/DungeonDoor");
    }

    public void OnActionSecondary(Info info)
    {
        if (info is not PlayerInfo) return; // only players can use the door
        // First use unlocks the quota system — the company starts counting.
        Save.Inst.mawUnlocked = true;
        // The Abyss-side door leads into the Maw; the Maw-side door leads home.
        Scene.SwitchWorld(Save.Inst.current == GenType.Maw ? GenType.Abyss : GenType.Maw);
    }
}
