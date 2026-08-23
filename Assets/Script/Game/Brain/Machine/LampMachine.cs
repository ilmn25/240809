/// <summary>A placeable lamp. Doesn't need power: right-click toggles its switch.
/// Furniture: placed directly, can't be broken, hammer picks it back up.</summary>
public class LampMachine : FurnitureMachine, IActionSecondaryInteract
{
    public static Info CreateInfo()
    {
        return CreateFurnitureInfo(ID.Lamp, glowOn: true);
    }

    public override void OnStart()
    {
        base.OnStart();
        SetGlow(Info is StructureInfo si && si.GlowOn);
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
