using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class GUIStorageSingleton : MonoBehaviour
{
    public static GUIStorageSingleton Instance { get; private set; }
    private int _currentSlotKey = -1; 
    public static event Action OnRefreshSlot;
 
    private int SLOT_SIZE = 30;
    private Vector2 MARGIN = new Vector2(10, 10);

    private void Start()
    {
        Instance = this;
        Initialize();
    }

    private void Initialize()
    {
        for (int i = 0;
             i < InventorySingleton.INVENTORY_SLOT_AMOUNT * InventorySingleton.INVENTORY_ROW_AMOUNT;
             i++)
        {
            GameObject slot = Instantiate(Resources.Load<GameObject>($"prefab/gui_item_slot"), Game.GUIInvStorage.transform, false);
    
            int row = i / InventorySingleton.INVENTORY_SLOT_AMOUNT;
            int column = i % InventorySingleton.INVENTORY_SLOT_AMOUNT;

            RectTransform slotRectTransform = slot.GetComponent<RectTransform>();
            slotRectTransform.sizeDelta = new Vector2(SLOT_SIZE, SLOT_SIZE);
            slotRectTransform.anchoredPosition = new Vector2(
                column * (SLOT_SIZE + MARGIN.x),
                -row * (SLOT_SIZE + MARGIN.y)
            );
            slot.AddComponent<GUIInvSlotModule>().SlotNumber = InventorySingleton.CalculateKey(row, column);
        }
    }

    public void RefreshCursorSlot()
    { OnRefreshSlot?.Invoke();}

    public void SetInfoPanel(int slotNumber = -1)
    {
        _currentSlotKey = slotNumber;
        
        if (slotNumber == -1)
        {
            GUICursorSingleton.SetInfoPanel();
            return;
        }
        InvSlotData slotData = InventorySingleton._playerInventory[_currentSlotKey];
        if (slotData.Stack != 0)
        { 
            GUICursorSingleton.SetInfoPanel(slotData.StringID + " (" + slotData.Stack + ")\n" + 
                                   ItemSingleton.GetItem(slotData.StringID).Description + "\n" +
                                   slotData.Modifier);
        }
        else
        {
            GUICursorSingleton.SetInfoPanel();
        }
    }
     
    private void Update()
    {
        if (_currentSlotKey == -1) return; 
  
        if (Input.GetMouseButtonDown(0))
        {
            if (GUICursorSingleton._cursorSlot.isEmpty())
            {
                GUICursorSingleton._cursorSlot.Add(InventorySingleton._playerInventory[_currentSlotKey]);
            }
            else if (InventorySingleton._playerInventory[_currentSlotKey].isSame(GUICursorSingleton._cursorSlot))
            {
                InventorySingleton._playerInventory[_currentSlotKey].Add(GUICursorSingleton._cursorSlot);
            } 
            else
            {
                (InventorySingleton._playerInventory[_currentSlotKey], GUICursorSingleton._cursorSlot) = 
                    (GUICursorSingleton._cursorSlot, InventorySingleton._playerInventory[_currentSlotKey]);
            } 
            GUICursorSingleton.UpdateCursorSlot();
            RefreshCursorSlot();
        }
        else if (Input.GetMouseButtonDown(1))
        {
            InvSlotData invSlot = InventorySingleton._playerInventory[_currentSlotKey];
            if (!invSlot.isEmpty())
            {
                if (GUICursorSingleton._cursorSlot.isEmpty() || invSlot.isSame(GUICursorSingleton._cursorSlot))
                {
                    if (Input.GetKey(KeyCode.LeftShift))
                        GUICursorSingleton._cursorSlot.Add(invSlot, invSlot.Stack/2);
                    else 
                        GUICursorSingleton._cursorSlot.Add(invSlot, 1);
                        
                    GUICursorSingleton.UpdateCursorSlot();
                    RefreshCursorSlot();
                }
            }
        }
    } 
}