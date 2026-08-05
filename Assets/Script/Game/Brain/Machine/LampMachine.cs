/// <summary>A placeable lamp. Right-click to toggle its light on and off.
/// Starts lit when placed; the state is host-authoritative and synced to clients.</summary>
public class LampMachine : StructureMachine, IActionSecondaryInteract
{
    public static Info CreateInfo()
    {
        return new StructureInfo
        {
            Health = 100,
            Loot = ID.Lamp,
            SfxHit = SfxID.HitStone,
            SfxDestroy = SfxID.HitStone,
            operationType = OperationType.Cutting,
            GlowOn = true,
        };
    }

    public void OnActionSecondary(Info info)
    {
        if (!(Info is StructureInfo structureInfo)) return;

        structureInfo.GlowOn = !structureInfo.GlowOn;
        SetGlow(structureInfo.GlowOn);
        Audio.PlaySFX(SfxID.Item);

        // Host broadcasts the new state to clients; a remote client relays
        // the toggle request to the host so it stays authoritative.
        LampSync.Toggle(this, structureInfo);
    }
}
