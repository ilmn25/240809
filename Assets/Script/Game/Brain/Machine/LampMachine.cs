/// <summary>A placeable lamp. Needs a nearby generator to light: right-click toggles
/// its switch while powered, otherwise it explains it needs power.</summary>
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
            Flammable = true,
        };
    }

    public override void OnStart()
    {
        base.OnStart();
        SetGlow(Powered && (Info is StructureInfo si && si.GlowOn));
    }

    public override void OnPoweredChanged(bool powered)
    {
        SetGlow(powered && (Info is StructureInfo si && si.GlowOn));
    }

    public void OnActionSecondary(Info info)
    {
        if (!(Info is StructureInfo structureInfo)) return;
        if (!Powered)
        {
            Dialogue.Target = new Dialogue { Text = "Needs an electric source." };
            Dialogue.Show(true);
            return;
        }

        structureInfo.GlowOn = !structureInfo.GlowOn;
        SetGlow(structureInfo.GlowOn);
        Audio.PlaySFX(SfxID.Item);

        // Host broadcasts the new state to clients; a remote client relays
        // the toggle request to the host so it stays authoritative.
        LampSync.Toggle(this, structureInfo);
    }
}
