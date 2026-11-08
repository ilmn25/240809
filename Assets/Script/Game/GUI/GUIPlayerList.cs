using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>A simple player list. Builds an empty parent in code and keeps a
/// pool of GUIPlayerSlot entries (one per player). Hovering a slot pops up that
/// player's info in the same tooltip items use. Clicking a free player takes
/// control of them; right-click releases their work order or toggles rest.</summary>
public class GUIPlayerList : GUI
{
    private const int PoolSize = 8;
    private const int SlotSize = 30;
    private const int Spacing = 16;
    private readonly List<GUIPlayerSlot> _slots = new List<GUIPlayerSlot>();

    /// <summary>The player whose slot the cursor is over, or null — drives the info tooltip
    /// the same way <see cref="GUIStorage.Hovered"/> drives item tooltips.</summary>
    public static PlayerInfo Hovered { get; private set; }

    public new void Initialize()
    {
        // Simple empty parent (no background), inside the inventory GUI.
        GameObject = new GameObject("GUIPlayerList", typeof(RectTransform));
        GameObject.transform.SetParent(Main.GUIInv.transform, false);
        GameObject.AddComponent<HoverModule>().GUI = this;
        Rect = GameObject.GetComponent<RectTransform>();
        Rect.localScale = Vector3.one * 0.9f;
        Rect.localRotation = Quaternion.Euler(-15, 0, 0);
        Position = new Vector2(-445, 185);
        base.Initialize();

        for (int i = 0; i < PoolSize; i++)
        {
            GameObject slot = Object.Instantiate(
                Resources.Load<GameObject>("Prefab/GUIPlayerSlot"),
                GameObject.transform, false);

            RectTransform slotRect = slot.GetComponent<RectTransform>();
            slotRect.sizeDelta = new Vector2(SlotSize, SlotSize);
            slotRect.anchoredPosition = new Vector2(i * (SlotSize + Spacing), 0);

            GUIPlayerSlot guiSlot = slot.AddComponent<GUIPlayerSlot>();
            guiSlot.GUIPlayerList = this;
            _slots.Add(guiSlot);
        }
    }

    public void Update()
    {
        // Hiding the list (or losing the save) can't be relied on to fire pointer exits, so
        // drop the hover here — otherwise reopening the inventory pops a stale tooltip.
        if (!GUIMain.Showing || !Showing || Save.Inst == null)
        {
            SetHovered(null);
            return;
        }

        int count = Save.Inst.players.Count;
        for (int i = 0; i < _slots.Count; i++)
        {
            _slots[i].Set(i < count ? Save.Inst.players[i] : null);
        }
    }

    /// <summary>Whether the local player currently controls the given player.</summary>
    public static bool IsControlling(PlayerInfo player)
    {
        if (player == null) return false;
        return Helper.IsHost()
            ? player.controllerId == 0
            : player.controllerId == PlayerSync.MyConnectionId;
    }

    /// <summary>Take control of the given player if it's free (or already ours).
    /// Returns true if control was taken.</summary>
    public bool TryControl(PlayerInfo player)
    {
        if (player == null || IsControlling(player)) return false;
        if (player.IsDelver) return false; // AI-only — cannot be selected
        if (player.controllerId != -1) return false; // controlled by someone else

        int index = Save.Inst.players.IndexOf(player);
        if (index < 0) return false;
        if (player.Resting) SetRest(player, false);
        Control.SwitchToPlayer(index);
        Audio.PlaySFX(SfxID.Text);
        return true;
    }

    /// <summary>Toggle whether the given player rests (stands still, regains
    /// health, and stops burning hunger from walking).</summary>
    public void ToggleRest(PlayerInfo player)
    {
        if (player == null) return;
        SetRest(player, !player.Resting);
    }

    /// <summary>Right-click on a slot: release the tree/rock job the player is working on
    /// (so an ally stops mining/cutting and goes back to following). With no such job it
    /// toggles rest instead.</summary>
    public void HandleRightClick(PlayerInfo player)
    {
        if (!player.ReleaseWork())
        {
            ToggleRest(player);
            return;
        }
        PlayerSync.SendReleaseWork(player.uid);
        Audio.PlaySFX(SfxID.Text);
    }

    /// <summary>Track which player's slot the cursor is over (null when it leaves) so the
    /// HUD can show that player's info, the same way hovering a slot shows an item's.</summary>
    public void SetHovered(PlayerInfo player) => Hovered = player;

    /// <summary>HUD hint while hovering a party member slot, matching what the clicks will
    /// actually do to that player — the player-list counterpart of the item-slot hint.</summary>
    public static string HoveredShortcuts(PlayerInfo player)
    {
        string rightClick = player.IsWorking ? "RMB: abandon task" : "RMB: stay still";
        return !player.IsDelver && player.controllerId == -1
            ? "LMB: take control | " + rightClick
            : rightClick;
    }

    private void SetRest(PlayerInfo player, bool resting)
    {
        if (player == null || player.Resting == resting) return;
        player.Resting = resting;
        Audio.PlaySFX(SfxID.Text);
        if (!Helper.IsHost())
            PlayerSync.SendRest(player.uid, resting);
    }
}
