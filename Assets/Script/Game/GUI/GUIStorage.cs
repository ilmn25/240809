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
    
    public Storage Storage; 
    public int RowAmount = 3;
    public int SlotAmount = 9;
    public Vector2 Position;
    public string Name;
    protected int CurrentSlotKey = -1;
    private GameObject _storageObject;
    private RectTransform _storageRect;
    private RectTransform _parentRect; 
    private GameObject _shadow;
    public bool IsDrag;
    private Vector2 _dragOffset;
 
 
    public void Initialize()
    { 
        _storageObject = Object.Instantiate(Resources.Load<GameObject>($"prefab/gui_storage"),
            Game.GUIInv.transform);
        _storageObject.name = Name;
        _storageObject.transform.Find("text").GetComponent<TextMeshProUGUI>().text = Name;
        _storageObject.GetComponent<HoverModule>().GUI = this;
        _storageRect = _storageObject.GetComponent<RectTransform>();
        _storageRect.anchoredPosition = Position;
        _parentRect = _storageRect.parent.GetComponent<RectTransform>();
            
        for (int i = 0; i < SlotAmount * RowAmount; i++)
        {
            GameObject slot = Object.Instantiate(Resources.Load<GameObject>($"prefab/gui_item_slot"),
                _storageObject.transform, false);

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
        // Storage = Inventory.Storage;
        if (CurrentSlotKey == -1 || IsDrag)
        {
            if (IsHover || IsDrag)
            { 
                if (Control.Inst.ActionPrimary.KeyDown())
                {
                    Position = _storageRect.anchoredPosition;
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        _parentRect,
                        Input.mousePosition,
                        Game.GUICamera,
                        out _dragOffset
                    );
                    IsDrag = true;
                }
                
                if (IsDrag && Control.Inst.ActionPrimary.Key())
                {
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        _parentRect,
                        Input.mousePosition,
                        Game.GUICamera,
                        out Vector2 mousePosition
                    );

                    _storageRect.anchoredPosition = mousePosition - _dragOffset + Position;
                }
                
                if (Control.Inst.ActionPrimary.KeyUp())
                {
                    IsDrag = false;
                }
            }
        } 
        else
        {
            if (Control.Inst.ActionPrimary.KeyDown())
            { 
                ActionPrimary(); 
            }
            else if (Control.Inst.ActionSecondary.KeyDown())
            { 
                ActionSecondary(); 
            }
        }
    }

    protected virtual void ActionPrimary() {}
    protected virtual void ActionSecondary() {} 
    public void SetInfoPanel(int currentSlotKey = -1)
    {
        SetSidePanel(currentSlotKey);
        CurrentSlotKey = currentSlotKey;
        
        if (currentSlotKey == -1)
        {
            GUICursor.SetInfoPanel();
            return;
        }
        ItemSlot slot = Storage.List[currentSlotKey];
        if (slot.Stack != 0)
        { 
            GUICursor.SetInfoPanel(slot.StringID + " (" + slot.Stack + ")\n" + 
                                   Item.GetItem(slot.StringID).Description + "\n" +
                                   slot.Modifier);
        }
        else
        {
            GUICursor.SetInfoPanel();
        }
    }
    protected virtual void SetSidePanel(int currentSlotKey = -1) { }
}

public class GUIInventory : GUIChest { }

public class GUIChest : GUIStorage
{
    protected override void ActionPrimary()
    {
        if (GUICursor.Data.isEmpty())
        {
            GUICursor.Data.Add(Storage.List[CurrentSlotKey]);
        }
        else if (Storage.List[CurrentSlotKey].isSame(GUICursor.Data))
        {
            Storage.List[CurrentSlotKey].Add(GUICursor.Data);
        } 
        else
        {
            (Storage.List[CurrentSlotKey], GUICursor.Data) = 
                (GUICursor.Data, Storage.List[CurrentSlotKey]);
        } 
        GUICursor.UpdateCursorSlot();
    }

    protected override void ActionSecondary()
    {
        ItemSlot itemSlot = Storage.List[CurrentSlotKey];
        if (!itemSlot.isEmpty())
        {
            if (GUICursor.Data.isEmpty() || itemSlot.isSame(GUICursor.Data))
            {
                if (Input.GetKey(KeyCode.LeftShift))
                    GUICursor.Data.Add(itemSlot, itemSlot.Stack/2);
                else 
                    GUICursor.Data.Add(itemSlot, 1);
                        
                GUICursor.UpdateCursorSlot();
            }
        }
    }
}