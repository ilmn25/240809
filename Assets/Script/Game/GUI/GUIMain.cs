using System;
using System.Collections;
using System.ComponentModel;
using TMPro;
using UnityEngine;

public static class GUIMain 
{ 
    private const float ShowDuration = 0.5f;
    private const float HideDuration = 0.2f;
    
    private static CoroutineTask _showTask; 
    public static GUIStorage StorageInv;
    public static GUIStorage Storage; 
    public static GUIStorage HandCrafting;
    public static GUIStorage Crafting;
    public static GUIConverter Converter;
    public static GUIInfoPanel InfoPanel;
    public static GUICursor Cursor;
    public static GUIMenu GUIMenu;
    public static GUILoad GUILoad;

    public static bool Showing = true;
    public static bool IsHover;
    public static void Initialize()
    {
        Inventory.SlotUpdate += RefreshStorage;
        // GUICraft.Initialize(); 
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
            Position = new Vector2(-91, 170), 
        };
        StorageInv.Initialize();
        
        Storage = new GUIChest()
        {
            RowAmount = Inventory.InventoryRowAmount,
            SlotAmount = Inventory.InventorySlotAmount,
            Position = new Vector2(-92, 74), 
        };
        Storage.Initialize();
        Storage.Show(false);

        Storage storage = new Storage(9)
        {
            Name = "Crafting",
        };
        storage.CreateAndAddItem(ID.Station);
        storage.CreateAndAddItem(ID.StoneHatchet);
        storage.CreateAndAddItem(ID.Hammer);
        storage.CreateAndAddItem(ID.Campfire);
        HandCrafting = new GUICraft()
        {
            Storage = storage,
            RowAmount = 1,
            SlotAmount = 9,
            Position = new Vector2(297, 169), 
        };
        HandCrafting.Initialize();
        
        Crafting = new GUICraft()
        {
            RowAmount = 1,
            SlotAmount = Inventory.InventorySlotAmount,
            Position = new Vector2(297, 77), 
        };
        Crafting.Initialize();
        Crafting.Show(false);
        
        Converter = new GUIConverter()
        {
            RowAmount = 1,
            SlotAmount = 3,
            Position = new Vector2(297, -15), 
        };
        Converter.Initialize();
        Converter.Show(false);
         
        
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
        HandCrafting.Update();
        Crafting.Update();
        Converter.Update();
        InfoPanel.UpdateDrag();

        if (Control.Inst.Inv.KeyDown())
        { 
            if (Showing)
                Show(false);
            else
                Show(true);
        }
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
        HandCrafting.OnRefreshSlot?.Invoke(HandCrafting, null);
        Crafting.OnRefreshSlot?.Invoke(Crafting, null);
        Converter.OnRefreshSlot?.Invoke(Converter, null);
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
