using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>A simple player list. Builds an empty parent in code and keeps a
/// pool of GUIPlayerSlot entries (one per player). Hovering a slot pops up the
/// info panel with the player's sprite and HP. Clicking a free player takes
/// control of them.</summary>
public class GUIPlayerList : GUI
{
    private const int PoolSize = 8;
    private const int SlotSize = 30;
    private const int Spacing = 16;
    private readonly List<GUIPlayerSlot> _slots = new List<GUIPlayerSlot>();

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
        if (!GUIMain.Showing || !Showing) return;
        if (Save.Inst == null) return;

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

    /// <summary>Human-readable control status for the given player.</summary>
    public static string ControlStatus(PlayerInfo player)
    {
        if (player == null) return "";
        if (IsControlling(player)) return "You";
        if (player.controllerId == -1) return "Free";
        return $"Player {player.controllerId + 1}";
    }

    public void ShowInfo(PlayerInfo player)
    {
        if (player == null)
        {
            HideInfo();
            return;
        }
        string effects = player.Machine?.GetModule<StatusEffectModule>()?.ActiveEffectsText() ?? "";
        GUIMain.Cursor.Set(
            $"HP {player.Health}/{player.HealthMax}",
            $"Controlled by: {ControlStatus(player)}\n" +
            (effects.Length > 0 ? "Effects: " + effects + "\n" : "") +
            (IsControlling(player) ? "(controlling)" : "Click to select"));
    }

    public void HideInfo()
    {
        GUIMain.Cursor.Set();
    }

    /// <summary>Take control of the given player if it's free (or already ours).
    /// Returns true if control was taken.</summary>
    public bool TryControl(PlayerInfo player)
    {
        if (player == null || IsControlling(player)) return false;
        if (player.controllerId != -1) return false; // controlled by someone else

        int index = Save.Inst.players.IndexOf(player);
        if (index < 0) return false;
        Control.SwitchToPlayer(index);
        Audio.PlaySFX(SfxID.Text);
        return true;
    }
}
