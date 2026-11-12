using UnityEngine;

/// <summary>The window that shows a party member's inventory. Looking at a bag is UI, so it
/// is driven from the viewer's screen instead of from the owner's machine: the owner keeps
/// following, fighting and working while its bag is open, and the window closes itself once
/// the viewer stops standing beside them.
/// </summary>
public static class ContainerWindow
{
    /// <summary>How close the viewer must be to the owner for the bag to open and stay open.</summary>
    public const float ProximityRange = 1.5f;

    /// <summary>Party member whose bag is on screen; null when no player bag is shown.</summary>
    public static PlayerInfo Owner { get; private set; }

    private static int _viewerIndex;

    public static bool IsShowing(PlayerInfo owner) =>
        owner != null && Owner == owner && GUIMain.Storage.Showing &&
        GUIMain.Storage.Storage == owner.Storage;

    public static void Toggle(PlayerInfo owner)
    {
        if (IsShowing(owner))
            Close();
        else
            Open(owner);
    }

    public static void Open(PlayerInfo owner)
    {
        // A pet carries nothing, so there is no bag to show.
        if (owner == null || !owner.HasInventory || Main.PlayerInfo == null) return;

        Audio.PlaySFX(SfxID.Text);
        if (owner.Storage != Main.PlayerInfo.Storage)
            Tutorial.OnTeammateInventory();

        GUIMain.Storage.Storage = owner.Storage;
        GUIMain.RefreshStorage();
        GUIMain.Show(true);
        GUIMain.Storage.Show(true);

        Owner = owner;
        _viewerIndex = Control.CurrentPlayerIndex;
    }

    public static void Close()
    {
        if (Owner != null && GUIMain.Storage.Showing && GUIMain.Storage.Storage == Owner.Storage)
            GUIMain.Storage.Show(false);
        Owner = null;
    }

    /// <summary>Drops the bag once the viewer is no longer standing next to the owner.</summary>
    public static void Update()
    {
        if (Owner == null) return;

        // Hidden by the player, covered by a full-screen menu, or taken over by another
        // window (a chest, a shop, a crafting station).
        if (!GUIMain.Showing || !GUIMain.Storage.Showing ||
            GUIMain.Storage.Storage != Owner.Storage || GUIMain.GUIMenu.Showing)
        {
            Close();
            return;
        }

        if (Owner.Destroyed || Owner.Machine == null || Owner.PlayerStatus == PlayerStatus.Incapacitated ||
            Main.Player == null || Main.PlayerInfo == null || _viewerIndex != Control.CurrentPlayerIndex)
        {
            Close();
            return;
        }

        if (Control.GetMovementAxis() != Vector2Int.zero ||
            Helper.SquaredDistance(Owner.Machine.transform.position, Main.Player.transform.position) >
            ProximityRange * ProximityRange)
            Close();
    }
}
