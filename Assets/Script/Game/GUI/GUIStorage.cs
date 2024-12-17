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
        for (int i = 0; i < InventorySingleton.InventorySlotAmount * InventorySingleton.InventoryRowAmount; i++)
        {
            GameObject slot = Object.Instantiate(Resources.Load<GameObject>($"prefab/gui_item_slot"), Game.GUIInvStorage.transform, false);
    
            int row = i / InventorySingleton.InventorySlotAmount;
            int column = i % InventorySingleton.InventorySlotAmount;

            RectTransform slotRectTransform = slot.GetComponent<RectTransform>();
            slotRectTransform.sizeDelta = new Vector2(SlotSize, SlotSize);
            slotRectTransform.anchoredPosition = new Vector2(
                column * (SlotSize + Margin.x) - 160,
                -row * (SlotSize + Margin.y)
            );
            slot.AddComponent<GUIInvSlotModule>().slotNumber = InventorySingleton.CalculateKey(row, column);
        }
    }
     
    public static void Update()
    {
        if (_currentSlotKey == -1) return; 
  
        if (Input.GetMouseButtonDown(0))
        {
            if (GUICursor.Data.isEmpty())
            {
                GUICursor.Data.Add(InventorySingleton.PlayerInventory[_currentSlotKey]);
            }
            else if (InventorySingleton.PlayerInventory[_currentSlotKey].isSame(GUICursor.Data))
            {
                InventorySingleton.PlayerInventory[_currentSlotKey].Add(GUICursor.Data);
            } 
            else
            {
                (InventorySingleton.PlayerInventory[_currentSlotKey], GUICursor.Data) = 
                    (GUICursor.Data, InventorySingleton.PlayerInventory[_currentSlotKey]);
            } 
            GUICursor.UpdateCursorSlot();
            RefreshCursorSlot();
        }
        else if (Input.GetMouseButtonDown(1))
        {
            InvSlotData invSlot = InventorySingleton.PlayerInventory[_currentSlotKey];
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
        InvSlotData slotData = InventorySingleton.PlayerInventory[_currentSlotKey];
        if (slotData.Stack != 0)
        { 
            GUICursor.SetInfoPanel(slotData.StringID + " (" + slotData.Stack + ")\n" + 
                                   Item.GetItem(slotData.StringID).Description + "\n" +
                                   slotData.Modifier);
        }
        else
        {
            GUICursor.SetInfoPanel();
        }
    } 
}