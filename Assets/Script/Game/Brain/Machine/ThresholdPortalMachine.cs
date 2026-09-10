using UnityEngine;

/// <summary>An upright threshold portal: one stands beside the overworld spawn and
/// its twin stands in the backrooms. Right-clicking either steps through to the other.
/// Its sprite comes from the ID (<c>Threshold</c> -> Sprite/Threshold).</summary>
public class ThresholdPortalMachine : StructureMachine, IActionSecondaryInteract
{
    private const int PortalHealth = 1000;

    public static Info CreateInfo()
    {
        return new StructureInfo
        {
            Health = PortalHealth,
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
        if (info is not PlayerInfo) return; // only players can use the portal
        // The two ends are the same structure: whichever side of the threshold the
        // player is standing on, they step through to the other.
        Scene.SwitchWorld(Save.Inst.current == GenType.Backrooms ? GenType.Abyss : GenType.Backrooms);
    }
}
