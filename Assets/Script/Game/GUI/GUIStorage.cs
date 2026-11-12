using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public abstract class GUIStorage : GUI
{  
    public EventHandler OnRefreshSlot;
    private const int SlotSize = 30;
    private readonly Vector2 _margin = new Vector2(10, 10);
    private int _cooldown;
    private int _cooldownSpeed = 40;    
    
    public Storage Storage; 
    public int RowAmount = 1;
    public int SlotAmount = 9;
    protected abstract string PanelSprite { get; }
    protected int CurrentSlotKey = -1;
    /// <summary>True while the mouse is over any storage slot (inventory management
    /// takes priority over cursor quick-actions).</summary>
    public static bool HoveringSlot => Hovered != null;
    /// <summary>The storage currently hovered (null when not hovering a slot).</summary>
    public static GUIStorage Hovered;

    public new void Initialize()
    {  
        GameObject = Object.Instantiate(Resources.Load<GameObject>($"Prefab/GUIStorage"),
            Main.GUIInv.transform);
        GameObject.transform.Find("Panel").GetComponent<Image>().sprite =
            Cache.LoadSprite(PanelSprite);
        GameObject.GetComponent<HoverModule>().GUI = this;
        Rect = GameObject.GetComponent<RectTransform>();
        base.Initialize();
        for (int i = 0; i < SlotAmount * RowAmount; i++)
        {
            GameObject slot = Object.Instantiate(Resources.Load<GameObject>($"Prefab/GUIItemSlot"),
                GameObject.transform, false);

            int row = i / SlotAmount;
            int column = i % SlotAmount;

            RectTransform slotRectTransform = slot.GetComponent<RectTransform>();
            slotRectTransform.sizeDelta = new Vector2(SlotSize, SlotSize);
            slotRectTransform.anchoredPosition = new Vector2(
                column * (SlotSize + _margin.x) - 160,
                -row * (SlotSize + _margin.y)
            );
            GUIStorageSlot guiStorageSlot = slot.AddComponent<GUIStorageSlot>();
            guiStorageSlot.slotNumber = row * SlotAmount + column;
            guiStorageSlot.GUIStorage = this;
        }
    }
 
    
    public void Update()
    {
        if (!GUIMain.Showing || !Showing)
        {
            if (CurrentSlotKey != -1)
                SetHoveredSlot();
            return;
        }

        if (ScaleTask is { Running: true }) return;
        if (CurrentSlotKey == -1 || IsDrag)
        {
            UpdateDrag();
        } 
        else
        {
            if (Control.Inst.ActionPrimary.KeyDown())
            { 
                ActionPrimaryDown(); 
            }
            else if (Control.Inst.ActionSecondary.KeyDown())
            { 
                ActionSecondaryDown(); 
            }
            else if (Control.Inst.ActionSecondary.Key())
            {
                if (_cooldown == 0)
                {
                    _cooldown = _cooldownSpeed;
                    if (_cooldownSpeed != 10) _cooldownSpeed -= 10;
                    ActionSecondaryKey(); 
                }
                else
                {
                    _cooldown--;
                }
            }
            else if (Control.Inst.ActionSecondary.KeyUp())
            { 
                _cooldownSpeed = 40;
                _cooldown = 0;
            }
        }
    }

    /// <summary>Pads a storage out to the number of slots this window draws. Pools are
    /// built with exactly as many slots as they hold, which is fewer than the window
    /// shows, so a station's storage must be fitted before it is displayed.</summary>
    public void FitSlots(Storage storage)
    {
        int count = SlotAmount * RowAmount;
        if (storage.List == null)
            storage.List = new List<ItemSlot>(count);

        while (storage.List.Count < count)
            storage.List.Add(new ItemSlot());
    }

    /// <summary>Show or hide the panel itself — a player with no bag (a pet) has no slots
    /// to draw.</summary>
    public void SetVisible(bool visible)
    {
        if (GameObject != null && GameObject.activeSelf != visible)
            GameObject.SetActive(visible);
    }

    protected virtual void ActionPrimaryDown() {}
    protected virtual void ActionSecondaryDown() {} 
    protected virtual void ActionSecondaryKey() {} 
    /// <summary>Tracks which slot the cursor is over so the HUD can describe it.
    /// Null (-1) when the cursor leaves the window.</summary>
    public void SetHoveredSlot(int currentSlotKey = -1)
    { 
        CurrentSlotKey = currentSlotKey;
        Hovered = currentSlotKey != -1 ? this : null;
    }
    public virtual string HoveredItemText
    {
        get
        {
            if (CurrentSlot is not { Stack: > 0 } slot) return "";
            return slot.Tooltip();
        }
    }

    /// <summary>Secondary block shown in the recipe label beside the tooltip: the recipe of a
    /// structure in plain storage, otherwise empty.</summary>
    public virtual string HoveredItemDetails
    {
        get
        {
            if (CurrentSlot is not { Stack: > 0 } slot) return "";
            if (slot.Info.Type != ItemType.Structure) return "";
            return ItemRecipe.GetRecipe(slot.ID)?.DescribeIngredients() ?? "";
        }
    }

    protected ItemSlot CurrentSlot =>
        CurrentSlotKey >= 0 && Storage != null && CurrentSlotKey < Storage.List.Count
            ? Storage.List[CurrentSlotKey]
            : null;
}