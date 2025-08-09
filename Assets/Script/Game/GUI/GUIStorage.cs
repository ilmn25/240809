using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class  GUIStorage 
{  
    public EventHandler OnRefreshSlot;
    private const int SlotSize = 30;
    private readonly Vector2 _margin = new Vector2(10, 10);
    
    public List<InvSlot> Storage; 
    public int RowAmount = 3;
    public int SlotAmount = 9;
    public Vector2 Position;
    protected int CurrentSlotKey = -1;

    private GameObject _storageObject;
    private RectTransform _storageRect;
    private RectTransform _parentRect; 
    private GameObject _shadow;
    public bool IsHover;
    public bool IsDrag;
    private Vector2 _dragOffset;
 
 
    public void Initialize()
    { 
        _storageObject = Object.Instantiate(Resources.Load<GameObject>($"prefab/gui_storage"),
            Game.GUIInv.transform);
        _storageObject.name = "gui_storage";
        _storageObject.GetComponent<GUIStorageModule>().GUIStorage = this;
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
            GUIItemSlot guiItemSlot = slot.AddComponent<GUIItemSlot>();
            guiItemSlot.slotNumber = row * SlotAmount + column;
            guiItemSlot.GUIStorage = this;
        }
    }

    public void Update()
    {
        Storage = Inventory.Storage;
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
        InvSlot slot = Storage[currentSlotKey];
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
            GUICursor.Data.Add(Storage[CurrentSlotKey]);
        }
        else if (Storage[CurrentSlotKey].isSame(GUICursor.Data))
        {
            Storage[CurrentSlotKey].Add(GUICursor.Data);
        } 
        else
        {
            (Inventory.Storage[CurrentSlotKey], GUICursor.Data) = 
                (GUICursor.Data, Storage[CurrentSlotKey]);
        } 
        GUICursor.UpdateCursorSlot();
    }

    protected override void ActionSecondary()
    {
        InvSlot invSlot = Storage[CurrentSlotKey];
        if (!invSlot.isEmpty())
        {
            if (GUICursor.Data.isEmpty() || invSlot.isSame(GUICursor.Data))
            {
                if (Input.GetKey(KeyCode.LeftShift))
                    GUICursor.Data.Add(invSlot, invSlot.Stack/2);
                else 
                    GUICursor.Data.Add(invSlot, 1);
                        
                GUICursor.UpdateCursorSlot();
            }
        }
    }
}