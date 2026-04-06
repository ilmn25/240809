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
    public static GUICursor Cursor;
    public static GUIMenu GUIMenu;
    public static GUILoad GUILoad;

    public static bool Showing = true;
    public static bool IsHover;
    public static void Initialize()
    {
        Inventory.SlotUpdate += RefreshStorage;
        GUIMenu = new GUIMenu(); 
        GUILoad = new GUILoad();
        GUILoad.Show(false);
        
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

        Storage storage = new NoRefreshStorage(9)
        {
            Name = "Crafting",
        }; 
        storage.CreateAndAddItem(ID.CrudePickaxe);
        storage.CreateAndAddItem(ID.CrudeHatchet);
        storage.CreateAndAddItem(ID.CrudeMallet);
        storage.CreateAndAddItem(ID.Station);
        storage.CreateAndAddItem(ID.Campfire);
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
        
        Dialogue.Show(false);
        Show(false);
    }

    public static void UpdateMenu()
    {
        GUIMenu.Update();
        GUILoad.Update();
    }

    public static void Update()
    { 
        Dialogue.Update(); 
        Cursor.Update();
        StorageInv.Update();
        Storage.Update();
        GUICraft.Update();
        InfoPanel.UpdateDrag();
        UpdateHudText();

        if (Control.Inst.Inv.KeyDown())
        { 
            Audio.PlaySFX(SfxID.Text);
            if (Showing)
                Show(false);
            else
                Show(true);
        }
    }

    private static void UpdateHudText()
    {
        string BuildTimeHudText()
        {
            int minutes = (SaveData.Inst.time + 300) % 1440; // 5:00 AM start, wrap at midnight
            int hours24 = minutes / 60;
            int hours12 = hours24 % 12 == 0 ? 12 : hours24 % 12;
            return $"Day {SaveData.Inst.day}, {hours12}:{minutes % 60:00} {(hours24 >= 12 ? "PM" : "AM")}";
        }

        string BuildTargetHudText(Info target)
        {
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
        Main.GUIHudText.text =
            $"{BuildTimeHudText()}\n" +
            $"Controlling Player {playerIndex} | Slot {slotId}\n" +
            BuildTargetHudText(Main.PlayerInfo?.Target);
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
                if (!GUICursor.Data.isEmpty())
                {
                    // try add to player storage; AddItem will drop leftover automatically
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
                // if (target.transform.localScale.x > 0.5f) _isInputBlocked = false;
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
