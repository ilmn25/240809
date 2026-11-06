using System.Collections;
using UnityEngine;

public static class GUIMain 
{ 
    private const float ShowDuration = 0.5f;
    private const float HideDuration = 0.2f;
    
    private static CoroutineTask _showTask;
    private static string _hoveredText;
    private static string _hoveredDetails;
    private static bool _menuWasShowing;
    public static GUIStorage StorageInv;
    public static GUIStorage Storage; 
    public static GUICraft GUICraft;
    public static GUICursor Cursor;
    public static GUIPlayerList PlayerList;
    public static GUIMenu GUIMenu;
    public static GUIMap Map;

    public static bool Showing = true;
    public static bool IsHover;
    /// <summary>True while the inventory is open and an item is held on the cursor;
    /// cursor quick-actions take over from world input.</summary>
    public static bool CursorActive => Showing && !GUICursor.Data.isEmpty();
    public static void Initialize()
    {
        Inventory.SlotUpdate += RefreshStorage;
        GUIMenu = new GUIMenu();
        GUIMenu.ShowMain();
        
        Cursor = new GUICursor();
        Cursor.Initialize();
        
        StorageInv = new GUIChest()
        {
            RowAmount = Inventory.InventoryRowAmount,
            SlotAmount = Inventory.InventorySlotAmount,
            Position = new Vector2(297, 110), 
        };
        StorageInv.Initialize();
        
        Storage = new GUIChest()
        {
            RowAmount = Inventory.InventoryRowAmount,
            SlotAmount = Inventory.InventorySlotAmount,
            Position = new Vector2(295, 18), 
        };
        Storage.Initialize();
        Storage.Show(false);

        Storage storage = CraftInfo.GetPlayerPool();
        GUICraft = new GUICraft()
        {
            Storage = storage,
            DefaultStorage = storage,
            RowAmount = 1,
            SlotAmount = 9,
            Position = new Vector2(300, 205), 
        };
        GUICraft.Initialize();
        
        PlayerList = new GUIPlayerList();
        PlayerList.Initialize();

        Map = new GUIMap();
        Map.Initialize();
        
        Dialogue.Show(false);
        Show(false);
    }

    public static void UpdateMenu()
    {
        // The cursor only follows in game mode, so hide it in the menu to avoid
        // a stuck, non-moving cursor.
        if (Main.GUICursor != null) Main.GUICursor.SetActive(false);
        // Menu screens own this corner of the HUD, and the tooltip update does not run
        // outside game mode, so the recipe label must be cleared here.
        if (GUIMenu.Showing) Main.GUIRecipe.gameObject.SetActive(false);
        GUIMenu.Update();
    }

    /// <summary>Call when leaving game mode to hide the inventory GUI.</summary>
    public static void OnGameEnd()
    {
        ContainerWindow.Close();
        Show(false);
    }

    public static void Update()
    {
        SyncHudVisibility();
        ContainerWindow.Update();

        // Re-show the cursor now that we're in game mode (hidden while in menu).
        if (Main.GUICursor != null) Main.GUICursor.SetActive(true);
        Dialogue.Update(); 
        Cursor.Update();
        if (Showing) Cursor.HandleInteraction();
        StorageInv.Update();
        Storage.Update();
        GUICraft.Update();
        PlayerList.Update();
        Map.Update();
        UpdateHudText();
        CheckAllDowned();

        if (Control.Inst.Inv.KeyDown())
        { 
            Audio.PlaySFX(SfxID.Text);
            Show(false);
        }

        if (Control.Inst.Map.KeyDown())
        {
            Audio.PlaySFX(SfxID.Text);
            bool wasOpen = Map.Showing;
            Map.Show(!wasOpen);
            // Refocus the map onto the player when opening it.
            if (!wasOpen) Map.FocusOnPlayer();
        }
    }

    // Shows the death menu when every player is downed (incapacitated).
    private static void CheckAllDowned()
    {
        if (Save.Inst == null || Save.Inst.players.Count == 0) return;
        if (GUIMenu.Showing) return;

        foreach (PlayerInfo player in Save.Inst.players)
        {
            // Delvers are AI-only — they can't "carry" the run, so only humans count.
            if (player == null || player.IsDelver) continue;
            if (player.PlayerStatus != PlayerStatus.Incapacitated)
                return;
        }

        GUIMenu.ShowDeath();
    }

    private static void UpdateHudText()
    {
        Tutorial.Update();

        string BuildTimeHudText()
        {
            if (Save.Inst == null) return "Day ?, ?";
            if (Save.Inst.time >= Environment.NightStartTime &&
                Save.Inst.weather is EnvironmentType.NightRainy or EnvironmentType.NightBright)
            {
                string nightType = Save.Inst.weather == EnvironmentType.NightBright
                    ? "Bright"
                    : "Rainy";
                return $"Day {Save.Inst.day}, {nightType} night\n{RaidEvent.GetCountdownText()}";
            }

            int minutesUntilNight = Mathf.Max(0, Environment.NightStartTime - Save.Inst.time);
            string countdown = Helper.FormatDuration(minutesUntilNight * Environment.SecondsPerGameMinute);
            return $"Day {Save.Inst.day}, {countdown} until night\n{RaidEvent.GetCountdownText()}";
        }

        string BuildCameraDirText()
        {
            if (Camera.main == null) return "";
            Vector3 fwd = Camera.main.transform.forward;
            fwd.y = 0;
            if (fwd.sqrMagnitude < 0.0001f) return "";
            fwd.Normalize();
            // North = -Z, East = +X, South = +Z, West = -X.
            float angle = Mathf.Atan2(fwd.x, -fwd.z) * Mathf.Rad2Deg;
            if (angle < 0) angle += 360f;
            string[] dirs = { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };
            return dirs[Mathf.RoundToInt(angle / 45f) % 8];
        }

        string BuildTargetHudText(Info target)
        {
            // Downed player: show status instead of the (cancelled) target.
            if (Main.PlayerInfo?.PlayerStatus == PlayerStatus.Incapacitated)
                return "Incapacitated";

            if (target == null)
                return "";

            string text = target.ToString();

            if (target is StructureInfo structureInfo &&
                Main.PlayerInfo?.Equipment?.Info?.ProjectileInfo != null &&
                Main.PlayerInfo.Equipment.Info.ProjectileInfo.Breaking < structureInfo.threshold)
            {
                text += " (tool too weak)";
            }

            return text;
        }

        // Mouse/inventory shortcuts, matching the actual slot/cursor behavior.
        string BuildShortcutsText()
        {
            if (GUIStorage.HoveringSlot)
            {
                // What a click on a slot does in the open window: a shop buys, a collector's
                // window sells, a crafting station crafts.
                if (GUIStorage.Hovered == GUIMain.GUICraft)
                    return $"LMB/hold RMB: {GUIMain.GUICraft.ActionLabel}";

                // Shift+LMB moves to the chest when hovering the player inventory with
                // the chest open; otherwise it drops the stack.
                string shiftLmb = GUIStorage.Hovered == GUIMain.StorageInv && GUIMain.Storage.Showing
                    ? "move"
                    : "drop";
                return $"Shift+LMB: {shiftLmb} | RMB: take one | Shift+RMB: split";
            }
            if (!GUICursor.Data.isEmpty())
            {
                string action = Inventory.CurrentItem?.Info.ActionLabel ?? "";
                return action.Length > 0
                    ? $"LMB: {action} | RMB: drop"
                    : "RMB: drop";
            }
            return "";
        }
    
        if (Scene.Busy)
        {
            Main.GUIHudText.text = string.Empty;
            return;
        }

        string uid = Main.PlayerInfo?.uid ?? "";
        bool isSpectating = Helper.IsHost()
            ? PlayerSync.IsClaimedByRemoteClient(uid)
            : !PlayerSync.CanLocalClientControl(uid);
        string controlStatus = isSpectating ? "spectating" : "";

        string tutorial = Tutorial.BuildHudText();
        string effects = Main.PlayerInfo?.Machine?.GetModule<StatusEffectModule>()?.ActiveEffectsText() ?? "";
        UpdateHoveredTooltip();
        Main.GUIHudText.text =
            $"{BuildTimeHudText()} | {BuildCameraDirText()}\n" +
            (controlStatus.Length > 0 ? controlStatus + "\n" : "") +
            BuildTargetHudText(Main.PlayerInfo?.Target) +
            (effects.Length > 0 ? "\nEffects: " + effects : "") +
            (tutorial.Length > 0 ? "\n" + tutorial : "") +
            "\n" + BuildShortcutsText();
    }

    private static void UpdateHoveredTooltip()
    {
        // Hovering only counts while the inventory GUI is open and no full-screen menu is up,
        // so a closed window or an opened menu cannot leave a tooltip behind.
        GUIStorage hovered = Showing && !GUIMenu.Showing ? GUIStorage.Hovered : null;
        string text = hovered?.HoveredItemText ?? "";
        string details = hovered?.HoveredItemDetails ?? "";

        bool changed = text != _hoveredText || details != _hoveredDetails ||
                       _menuWasShowing != GUIMenu.Showing;
        _hoveredText = text;
        _hoveredDetails = details;
        _menuWasShowing = GUIMenu.Showing;
        if (!changed) return;

        bool hovering = text.Length > 0;
        Main.GUIMenu.gameObject.SetActive(GUIMenu.Showing || hovering);
        Main.GUIMenu.text = text;
        // The recipe/price label keeps the same spot for every hovered item, even when it
        // has nothing to show, so the tooltip never shifts.
        Main.GUIRecipe.gameObject.SetActive(hovering);
        Main.GUIRecipe.text = details;
    }

    public static void Show(bool isShow)
    {
        if (isShow)
        {
            if (Intermission.Active) return;
            if (!Showing)
            {
                Showing = true;
                RefreshStorage();
                _showTask?.Stop();
                _showTask = new CoroutineTask(Scale(true, ShowDuration, Main.GUIInv, 0.7f));
            }
        }
        else
        {
            if (Showing)
            {
                Showing = false;

                // return any item held on cursor to inventory (or drop if full)
                if (!GUICursor.Data.isEmpty() && Main.PlayerInfo?.Storage != null)
                {
                    Main.PlayerInfo.Storage.AddItem(GUICursor.Data);
                    GUICursor.UpdateCursorSlot();
                }

                _showTask?.Stop();
                _showTask = new CoroutineTask(Scale(false, HideDuration, Main.GUIInv, 0));
                // _showTask.Finished += (bool isManual) => 
                // {
                //     Game.GUIInv.SetActive(false);
                // };
            }
        }
    }

    public static void RefreshStorage()
    {
        StorageInv.OnRefreshSlot?.Invoke(StorageInv, null);
        Storage.OnRefreshSlot?.Invoke(Storage, null);
        GUICraft.OnRefreshSlot?.Invoke(GUICraft, null);
        GUICursor.UpdateCursorSlot();
    }

    /// <summary>Keeps the HUD visible only while the controlled player is alive.
    /// Central source of truth: covers death, revival, and swapping between
    /// alive/dead characters from one place.</summary>
    public static void SyncHudVisibility()
    {
        bool alive = Main.PlayerInfo != null &&
                     Main.PlayerInfo.PlayerStatus != PlayerStatus.Incapacitated;
        Show(alive);
    }
    public static IEnumerator Scale(bool show, float duration, GameObject target, float scale, float easeSpeed = 0.5f)
    { 
        Vector3 initialScale = target.transform.localScale;
        Vector3 targetScale = Vector3.one * scale;
        float elapsedTime = 0f;

        while (elapsedTime < duration * 0.98f)
        {
            float t = elapsedTime / duration;
            if (show)
            {
                t = Mathf.SmoothStep(0f, 1f, Mathf.Pow(t, easeSpeed)); // Apply adjustable ease-out effect
            }
            else
            {
                t = Mathf.Lerp(0f, 1f, t); // Linear interpolation for hiding
            }

            target.transform.localScale = Vector3.Lerp(initialScale, targetScale, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        target.transform.localScale = targetScale;
    }
    
    public static IEnumerator Slide(bool show, float duration, GameObject target, Vector3 position, float easeSpeed = 0.5f)
    {
        Vector3 initialPos = target.transform.localPosition;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;

            if (show)
            {
                t = Mathf.SmoothStep(0f, 1f, Mathf.Pow(t, easeSpeed)); // Ease-out
            }
            else
            {
                t = Mathf.Lerp(0f, 1f, t); // Linear for hiding
            }

            target.transform.localPosition = Vector3.Lerp(initialPos, position, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        target.transform.localPosition = position;
    }
}
