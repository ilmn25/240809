using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public abstract class GUIStorage : GUI
{  
    public EventHandler OnRefreshSlot;
    private const int SlotSize = 30;
    private readonly Vector2 _margin = new Vector2(10, 10);
    private int _cooldown = 0;
    private int _cooldownSpeed = 40;    
    
    public Storage Storage; 
    public int RowAmount = 1;
    public int SlotAmount = 9;
    protected abstract string PanelSprite { get; }
    protected int CurrentSlotKey = -1;
    /// <summary>True while the mouse is over any storage slot (inventory management
    /// takes priority over cursor quick-actions).</summary>
    public static bool HoveringSlot;
    /// <summary>The storage currently hovered (null when not hovering a slot).</summary>
    public static GUIStorage Hovered;

    private void OnRefresh(object sender, EventArgs e)
    {
        if (Storage != null)
            Text.text = Storage.DisplayName;
    }
    public new void Initialize()
    {  
        GameObject = Object.Instantiate(Resources.Load<GameObject>($"Prefab/GUIStorage"),
            Main.GUIInv.transform);
        Text = GameObject.transform.Find("Text").GetComponent<TextMeshProUGUI>();
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
        OnRefreshSlot += OnRefresh;
    }
 
    
    public void Update()
    {
        if (!GUIMain.Showing || !Showing)
        {
            if (CurrentSlotKey != -1)
                SetInfoPanel();
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

    protected virtual void ActionPrimaryDown() {}
    protected virtual void ActionSecondaryDown() {} 
    protected virtual void ActionSecondaryKey() {} 
    public void SetInfoPanel(int currentSlotKey = -1)
    { 
        CurrentSlotKey = currentSlotKey;
        HoveringSlot = currentSlotKey != -1;
        Hovered = currentSlotKey != -1 ? this : null;
        
        if (currentSlotKey == -1)
        {
            GUIMain.InfoPanel.Set();
            return;
        } 
        GUIMain.InfoPanel.Set();
    }
    public virtual string HoveredItemText
    {
        get
        {
            return CurrentSlot is not { Stack: > 0 } slot
                ? ""
                : $"{slot.Info.Name}\n{slot.ToString(false)}";
        }
    }

    protected ItemSlot CurrentSlot =>
        CurrentSlotKey >= 0 && Storage != null && CurrentSlotKey < Storage.List.Count
            ? Storage.List[CurrentSlotKey]
            : null;
}