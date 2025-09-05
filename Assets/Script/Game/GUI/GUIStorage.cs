using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

public class  GUIStorage : GUI
{  
    public EventHandler OnRefreshSlot;
    private const int SlotSize = 30;
    private readonly Vector2 _margin = new Vector2(10, 10);
    private int _cooldown = 0;
    private int _cooldownSpeed = 40;
    
    public Storage Storage; 
    public int RowAmount = 1;
    public int SlotAmount = 9;
    private string Name => Storage.Name ?? Storage.info.id.ToString();
    protected int CurrentSlotKey = -1;  

    private void OnRefresh(object sender, EventArgs e)
    {
        if (Storage != null)
            Text.text = Name;
    }
    public new void Initialize()
    {  
        GameObject = Object.Instantiate(Resources.Load<GameObject>($"Prefab/GUIStorage"),
            Game.GUIInv.transform);
        Text = GameObject.transform.Find("Text").GetComponent<TextMeshProUGUI>();
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
    
    protected virtual void ActionPrimaryDown() {}
    protected virtual void ActionSecondaryDown() {} 
    protected virtual void ActionSecondaryKey() {} 
    public void SetInfoPanel(int currentSlotKey = -1)
    { 
        CurrentSlotKey = currentSlotKey;
        
        if (currentSlotKey == -1)
        {
            GUIMain.Cursor.SetItemSlotInfo();
            GUIMain.InfoPanel.Set();
            return;
        } 
        ItemSlot slot = Storage.List[currentSlotKey];
        if (slot.Stack != 0)
        { 
            SetInfoPanel(slot);
            return;
        }
        GUIMain.Cursor.SetItemSlotInfo();
        GUIMain.InfoPanel.Set();
    }
    protected virtual void SetInfoPanel(ItemSlot itemSlot) { }
}