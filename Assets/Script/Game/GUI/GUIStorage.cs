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
    public string Name;
    protected int CurrentSlotKey = -1; 
 
 
    public new void Initialize()
    { 
        GameObject = Object.Instantiate(Resources.Load<GameObject>($"prefab/gui_storage"),
            Game.GUIInv.transform);
        GameObject.name = Name;
        GameObject.transform.Find("text").GetComponent<TextMeshProUGUI>().text = Name;
        GameObject.GetComponent<HoverModule>().GUI = this;
        Rect = GameObject.GetComponent<RectTransform>();
        base.Initialize();
        for (int i = 0; i < SlotAmount * RowAmount; i++)
        {
            GameObject slot = Object.Instantiate(Resources.Load<GameObject>($"prefab/gui_item_slot"),
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
        if (CurrentSlotKey == -1 || IsDrag)
        {
            UpdateDrag();
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
        CurrentSlotKey = currentSlotKey;
        
        if (currentSlotKey == -1)
        {
            GUIMain.Cursor.Set();
            GUIMain.InfoPanel.Set();
            return;
        } 
        ItemSlot slot = Storage.List[currentSlotKey];
        if (slot.Stack != 0)
        { 
            SetInfoPanel(slot);
            return;
        }
        GUIMain.Cursor.Set();
        GUIMain.InfoPanel.Set();
    }
    protected virtual void SetInfoPanel(ItemSlot itemSlot) { }
}

public class GUICrafting : GUIStorage
{
    protected override void ActionPrimary()
    {
        ItemRecipe.CraftItem(Storage.List[CurrentSlotKey].StringID);
        GUICursor.UpdateCursorSlot();
    }

    protected override void SetInfoPanel(ItemSlot itemSlot)
    {
        Item itemData = Item.GetItem(itemSlot.StringID);
        ItemRecipe itemRecipeData = ItemRecipe.GetRecipe(itemSlot.StringID);
        
        string text = itemData.Name + " (" + itemRecipeData.Stack + ")\n";
        foreach (var ingredient in itemRecipeData.Ingredients)
        {
            text += $"{ingredient.Key} ({ingredient.Value})\n";
        }
        text += itemData.Description + "\n";
        
        GUIMain.InfoPanel.Set(text);
    }
}


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

    protected override void SetInfoPanel(ItemSlot itemSlot)
    {
        GUIMain.Cursor.Set(itemSlot.StringID + " (" + itemSlot.Stack + ")\n" + 
                                       Item.GetItem(itemSlot.StringID).Description + "\n" +
                                       itemSlot.Modifier);
    }
}