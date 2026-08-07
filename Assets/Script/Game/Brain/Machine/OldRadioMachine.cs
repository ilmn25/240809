using UnityEngine;

/// <summary>A placeable old radio — one of the key items a travelling merchant looks for.
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
