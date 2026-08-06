using UnityEngine;

/// <summary>A placeable old radio. Purely a marker: a nearby bed (with a lamp too) uses it
/// as part of the requirements to host a travelling merchant. See BedMachine.
/// Interacting with it just makes the radio drowse.</summary>
public class OldRadioMachine : StructureMachine, IActionSecondaryInteract
{
    public static Info CreateInfo()
    {
        return new StructureInfo
        {
            Health = 100,
            Loot = ID.OldRadio,
            SfxHit = SfxID.HitStone,
            SfxDestroy = SfxID.HitStone,
        };
    }

    public void OnActionSecondary(Info info)
    {
        if (Dialogue.Showing) return;
        Dialogue.Target = new Dialogue { Text = "zzzZzZ..." };
        Dialogue.Show(true);
        Audio.PlaySFX(SfxID.Notification);
    }
}
