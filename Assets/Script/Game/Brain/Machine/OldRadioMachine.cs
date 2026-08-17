using UnityEngine;

/// <summary>A placeable old radio — one of the key items a travelling merchant looks for.
/// Furniture: placed directly, can't be broken, hammer picks it back up. Interacting
/// with it just makes the radio drowse.</summary>
public class OldRadioMachine : FurnitureMachine, IActionSecondaryInteract
{
    public static Info CreateInfo()
    {
        return CreateFurnitureInfo(ID.OldRadio);
    }

    public void OnActionSecondary(Info info)
    {
        if (Dialogue.Showing) return;
        Dialogue.Target = new Dialogue { Text = "zzzZzZ..." };
        Dialogue.Show(true);
        Audio.PlaySFX(SfxID.Notification);
    }
}
