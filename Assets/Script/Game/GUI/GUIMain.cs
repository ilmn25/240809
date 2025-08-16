using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class GUIMain 
{ 
    private const float ShowDuration = 0.5f;
    private const float HideDuration = 0.2f;
    
    private static CoroutineTask _showTask; 
    public static GUIStorage StorageInv;
    public static GUIStorage Storage; 
    public static GUIStorage HandCrafting;
    public static GUIStorage Crafting;
    public static GUIStorage Building;
    public static GUIInfoPanel InfoPanel;
    public static GUICursor Cursor;

    public static bool Showing = true;
    public static bool IsHover;
    public static void Initialize()
    {
        Inventory.SlotUpdate += RefreshStorage;
        GUICraft.Initialize(); 
        Cursor = new GUICursor();
        Cursor.Initialize();
        Cursor.Show(false);
        
        StorageInv = new GUIChest()
        {
            RowAmount = Inventory.InventoryRowAmount,
            SlotAmount = Inventory.InventorySlotAmount,
            Position = new Vector2(0, 166), 
            Name = "Inventory",
        };
        StorageInv.Initialize();
        
        Storage = new GUIChest()
        {
            RowAmount = Inventory.InventoryRowAmount,
            SlotAmount = Inventory.InventorySlotAmount,
            Position = new Vector2(0, -19), 
            Name = "Storage",
        };
        Storage.Initialize();
        Storage.Show(false);

        Storage storage = new Storage(18);
        storage.AddItem("station");
        storage.AddItem("hammer");
        storage.AddItem("spear");
        HandCrafting = new GUIHandCrafting()
        {
            Storage = storage,
            RowAmount = 2,
            SlotAmount = 9,
            Position = new Vector2(-100, -50), 
            Name = "Crafting",
        };
        HandCrafting.Initialize();
        
        Crafting = new GUICrafting()
        {
            RowAmount = Inventory.InventoryRowAmount,
            SlotAmount = Inventory.InventorySlotAmount,
            Position = new Vector2(0, -50), 
            Name = "Crafting Station",
        };
        Crafting.Initialize();
        Crafting.Show(false);
        
        Building = new GUIBuilding()
        {
            RowAmount = Inventory.InventoryRowAmount,
            SlotAmount = Inventory.InventorySlotAmount,
            Position = new Vector2(0, -100), 
            Name = "Building Station",
        };
        Building.Initialize();
        Building.Show(false);
        
        InfoPanel = new GUIInfoPanel();
        InfoPanel.Initialize();
        InfoPanel.Show(false);
        
        GUIDialogue.Show(false);
        Show(false);
    }
 
    public static void Update()
    {
        GUIDialogue.Update();
        GUICraft.Update(); 
        Cursor.Update();
        StorageInv.Update();
        Storage.Update();
        HandCrafting.Update();
        Crafting.Update();
        Building.Update();
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
                _showTask = new CoroutineTask(Scale(true, ShowDuration, Game.GUIInv, 0.7f));
            }
        }
        else
        {
            if (Showing)
            {
                Showing = false;
                _showTask?.Stop();
                _showTask = new CoroutineTask(Scale(false, HideDuration, Game.GUIInv, 0));
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
        Building.OnRefreshSlot?.Invoke(Building, null);
        GUICursor.UpdateCursorSlot();
    }

    public static IEnumerator ScrollText(string line, TextMeshProUGUI textBox, int speed = 75)
    {
        textBox.text = ""; 
    
        foreach (var letter in line.ToCharArray())        
        { 
            textBox.text += letter;
            yield return new WaitForSeconds(1f / speed);
        }
      
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
