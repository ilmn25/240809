using System;
using UnityEngine;
using Object = UnityEngine.Object;

public class  GUIStorage 
{ 
    public static event Action OnRefreshSlot;

    private const int SlotSize = 30;
    private static readonly Vector2 Margin = new Vector2(10, 10);
 
    private static int _currentSlotKey = -1; 

    public static void Initialize()
    {
        for (int i = 0; i < Inventory.InventorySlotAmount * Inventory.InventoryRowAmount; i++)
        {
            GameObject slot = Object.Instantiate(Resources.Load<GameObject>($"prefab/gui_item_slot"), Game.GUIInvStorage.transform, false);
    
            int row = i / Inventory.InventorySlotAmount;
            int column = i % Inventory.InventorySlotAmount;

            RectTransform slotRectTransform = slot.GetComponent<RectTransform>();
            slotRectTransform.sizeDelta = new Vector2(SlotSize, SlotSize);
            slotRectTransform.anchoredPosition = new Vector2(
                column * (SlotSize + Margin.x) - 160,
                -row * (SlotSize + Margin.y)
            );
            slot.AddComponent<GUIInvSlotModule>().slotNumber = Inventory.CalculateKey(row, column);
        }
    }
     
    public static void Update()
    {
        if (_currentSlotKey == -1) return; 
  
        if (Input.GetMouseButtonDown(0))
        {
            if (GUICursor.Data.isEmpty())
            {
                GUICursor.Data.Add(Inventory.PlayerInventory[_currentSlotKey]);
            }
            else if (Inventory.PlayerInventory[_currentSlotKey].isSame(GUICursor.Data))
            {
                Inventory.PlayerInventory[_currentSlotKey].Add(GUICursor.Data);
            } 
            else
            {
                (Inventory.PlayerInventory[_currentSlotKey], GUICursor.Data) = 
                    (GUICursor.Data, Inventory.PlayerInventory[_currentSlotKey]);
            } 
            GUICursor.UpdateCursorSlot();
            RefreshCursorSlot();
        }
        else if (Input.GetMouseButtonDown(1))
        {
            InvSlot invSlot = Inventory.PlayerInventory[_currentSlotKey];
            if (!invSlot.isEmpty())
            {
                if (GUICursor.Data.isEmpty() || invSlot.isSame(GUICursor.Data))
                {
                    if (Input.GetKey(KeyCode.LeftShift))
                        GUICursor.Data.Add(invSlot, invSlot.Stack/2);
                    else 
                        GUICursor.Data.Add(invSlot, 1);
                        
                    GUICursor.UpdateCursorSlot();
                    RefreshCursorSlot();
                }
            }
        }
    } 
    
    public static void RefreshCursorSlot()
    { OnRefreshSlot?.Invoke();}

    public static void SetInfoPanel(int slotNumber = -1)
    {
        _currentSlotKey = slotNumber;
        
        if (slotNumber == -1)
        {
            GUICursor.SetInfoPanel();
            return;
        }
        InvSlot slot = Inventory.PlayerInventory[_currentSlotKey];
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
}