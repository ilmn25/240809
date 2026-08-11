using System.Collections;
using UnityEngine;

public static class GUIMain 
{ 
    private const float ShowDuration = 0.5f;
    private const float HideDuration = 0.2f;
    
    private static CoroutineTask _showTask;
    public static GUIStorage StorageInv;
    public static GUIStorage Storage; 
    public static GUICraft GUICraft;
    public static GUIInfoPanel InfoPanel;
    public static GUIPlayerList PlayerList;
    public static GUICursor Cursor;
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
        Cursor.Show(false);
        
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
         
        
        InfoPanel = new GUIInfoPanel();
        InfoPanel.Initialize();
        InfoPanel.Show(false);
        
        PlayerList = new GUIPlayerList();
        PlayerList.Initialize();

        Map = new GUIMap();
        Map.Initialize();
        
        Dialogue.Show(false);
        Show(false);
    }

    public static void UpdateMenu()
    {
        GUIMenu.Update();
    }

    /// <summary>Call when leaving game mode to hide the inventory GUI.</summary>
    public static void OnGameEnd()
    {
        Show(false);
    }

    public static void Update()
    {

        Dialogue.Update(); 
        Cursor.Update();
        if (Showing) Cursor.HandleInteraction();
        StorageInv.Update();
        Storage.Update();
        GUICraft.Update();
        PlayerList.Update();
        InfoPanel.UpdateDrag();
        Map.Update();
        UpdateHudText();

        if (Control.Inst.Inv.KeyDown())
        { 
            Audio.PlaySFX(SfxID.Text);
            Show(false);
        }

        if (Control.Inst.Map.KeyDown())
        {
            Audio.PlaySFX(SfxID.Text);
            Tutorial.OnControl(Tutorial.TutorialControl.Map);
            bool wasOpen = Map.Showing;
            Map.Show(!wasOpen);
            // Refocus the map onto the player when opening it.
            if (!wasOpen) Map.FocusOnPlayer();
        }
    }

    private static void UpdateHudText()
    {
        Tutorial.Update();

        string BuildTimeHudText()
        {
            if (Save.Inst == null) return "Day ?, ??:??";
            return $"Day {Save.Inst.day}, {Helper.FormatTime(Save.Inst.time)}";
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
    
        if (Scene.Busy)
        {
            Main.GUIHudText.text = string.Empty;
            return;
        }

        int playerIndex = Control.CurrentPlayerIndex + 1;
        int slotId = Main.PlayerInfo?.Storage != null ? Main.PlayerInfo.Storage.Key + 1 : 1;

        string uid = Main.PlayerInfo?.uid ?? "";
        bool isSpectating = Helper.IsHost()
            ? PlayerSync.IsClaimedByRemoteClient(uid)
            : !PlayerSync.CanLocalClientControl(uid);
        string controlStatus = isSpectating
            ? $"Spectating Player {playerIndex}"
            : $"Controlling Player {playerIndex}";

        string tutorial = Tutorial.BuildHudText();
        Main.GUIHudText.text =
            $"{BuildTimeHudText()}\n" +
            $"{controlStatus} | Slot {slotId}\n" +
            BuildTargetHudText(Main.PlayerInfo?.Target) +
            (tutorial.Length > 0 ? "\n" + tutorial : "");
    }

    public static void Show(bool isShow)
    {
        if (isShow)
        {
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
                Cursor.SetItemSlotInfo();
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
