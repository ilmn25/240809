using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class GUIInventorySingleton : MonoBehaviour
{
    public static GUIInventorySingleton Instance { get; private set; }
    private int _currentSlotKey;
    public static event Action OnRefreshSlot;

    private int SLOT_SIZE = 30;
    private float SHOW_DURATION = 0.5f;
    private float HIDE_DURATION = 0.2f;
    private Vector2 OFFSET = new Vector2(-160, 30);
    private Vector2 MARGIN = new Vector2(5, 5);

    private CoroutineTask _scaleTask;
    private RectTransform _inventoryRect;
    private RectTransform _cursorRect;
    private TextMeshProUGUI _cursorInfoText; 
    private TextMeshProUGUI _cursorSlotText;
    private Image _cursorSlotImage;


    private void Start()
    {
        Instance = this;
        _cursorRect = Game.GUIInventoryCursor.GetComponent<RectTransform>();
        _cursorInfoText = Game.GUIInventoryCursorInfo.transform.Find("text").GetComponent<TextMeshProUGUI>();
        _inventoryRect = Game.GUIInventory.GetComponent<RectTransform>();
        _cursorSlotText = Game.GUIInventoryCursorSlot.transform.Find("text").GetComponent<TextMeshProUGUI>();
        _cursorSlotImage = Game.GUIInventoryCursorSlot.transform.Find("image").GetComponent<Image>();
        Initialize();
    }

    private void Initialize()
    {
        Transform slotParent = Game.GUIInventory.transform.Find("slot");
        for (int i = 0;
             i < PlayerInventorySingleton.INVENTORY_SLOT_AMOUNT * PlayerInventorySingleton.INVENTORY_ROW_AMOUNT;
             i++)
        {
            GameObject slot = Instantiate(Resources.Load<GameObject>($"prefab/gui_item_slot"), slotParent, false);

            int row = i / PlayerInventorySingleton.INVENTORY_SLOT_AMOUNT;
            int column = i % PlayerInventorySingleton.INVENTORY_SLOT_AMOUNT;

            RectTransform slotRectTransform = slot.GetComponent<RectTransform>();
            slotRectTransform.sizeDelta = new Vector2(SLOT_SIZE, SLOT_SIZE);
            slotRectTransform.anchoredPosition = new Vector2(
                column * (SLOT_SIZE + MARGIN.x) + OFFSET.x,
                -row * (SLOT_SIZE + MARGIN.y) + OFFSET.y
            );
            slot.GetComponent<GUIItemSlotModule>().SlotNumber = PlayerInventorySingleton.CalculateKey(row, column);
        }
    }

    public void Refresh()
    { OnRefreshSlot?.Invoke();}

    public void SetInfoPanel(int slotNumber = -1)
    {
        _currentSlotKey = slotNumber;
        if (slotNumber == -1)
        {  
            Game.GUIInventoryCursorInfo.SetActive(false);
        }
        else if (PlayerInventorySingleton._playerInventory.TryGetValue(slotNumber, out InvSlotData slotData))
        { 
            Game.GUIInventoryCursorInfo.SetActive(true);
            _cursorInfoText.text = slotData.StringID + "\n" + ItemLoadSingleton.GetItem(slotData.StringID).Description;
        }
    }
    private InvSlotData _cursorSlot = new InvSlotData("", 0);

    private void UpdateCursorSlot()
    {
        if (_cursorSlot.Stack == 0)
        {
            Game.GUIInventoryCursorSlot.SetActive(false);
        }
        else
        {
            Game.GUIInventoryCursorSlot.SetActive(true);
            _cursorSlotImage.sprite = Resources.Load<Sprite>($"texture/sprite/{_cursorSlot.StringID}");
            _cursorSlotText.text = _cursorSlot.Stack.ToString();
        } 
    }

    private void swapSlotData(InvSlotData Slot1, InvSlotData Slot2)
    {
        String tempString;
        bool tempBool;
        int tempInt;
        
        tempString = Slot1.StringID;
        Slot1.StringID = Slot2.StringID;
        Slot2.StringID = tempString;
        
        tempString = Slot1.Modifier;
        Slot1.Modifier = Slot2.Modifier;
        Slot2.Modifier = tempString;
        
        tempBool = Slot1.Locked;
        Slot1.Locked = Slot2.Locked;
        Slot2.Locked = tempBool;
        
        tempInt = Slot1.Stack;
        Slot1.Stack = Slot2.Stack;
        Slot2.Stack = tempInt;
    }
    
    private void HandleInput()
    {
        if (_currentSlotKey == -1) return; 
        if (Input.GetMouseButtonDown(0))
        { 
            PlayerInventorySingleton._playerInventory.TryGetValue(_currentSlotKey, out InvSlotData slotData);
            Lib.Log(_cursorSlot.Stack , slotData);
            if (_cursorSlot.Stack != 0 && slotData == null) // empty slot (add)
            {
                slotData = new InvSlotData(_cursorSlot.Stack, _cursorSlot.StringID, _cursorSlot.Modifier, _cursorSlot.Locked);
                PlayerInventorySingleton._playerInventory.Add(_currentSlotKey, slotData);
                _cursorSlot.clear();
                UpdateCursorSlot();
                Refresh();
            }
            else if (_cursorSlot.Stack != 0 && slotData != null) // both not empty (swap)
            {
                swapSlotData(slotData, _cursorSlot);
                UpdateCursorSlot();
                Refresh();
            }
            else if (_cursorSlot.Stack == 0 && slotData != null)
            { 
                _cursorSlot.SetItem(slotData.Stack, slotData.StringID, slotData.Modifier, slotData.Locked); 
                PlayerInventorySingleton.ModifySlot(_currentSlotKey, -slotData.Stack);
                UpdateCursorSlot();
                Refresh();
            } 
        }

        if (Input.GetMouseButtonDown(1))
        {
            if (PlayerInventorySingleton._playerInventory.TryGetValue(_currentSlotKey, out InvSlotData slotData))
            {  
                _cursorSlot.SetItem(slotData.Stack/2, slotData.StringID, slotData.Modifier, slotData.Locked);
                PlayerInventorySingleton.ModifySlot(_currentSlotKey, slotData.Stack / 2);
                UpdateCursorSlot();
                Refresh();
            } 
        }
    }
    
    private void Update()
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_inventoryRect, Input.mousePosition,  Game.GUICamera,out Vector2 mousePosition);
        _cursorRect.anchoredPosition = mousePosition;
        HandleInput();
        if (Input.GetKeyDown(KeyCode.Tab))  
        {
            if (_scaleTask == null || (_scaleTask != null && !_scaleTask.Running))
            {
                if (!Game.GUIInventory.activeSelf)
                {
                    Camera.main.depth = -1;
                    Game.GUIBusy = true;
                    Game.GUIInventory.SetActive(true);
                    Refresh();
                    _scaleTask = new CoroutineTask(GUISingleton.Scale(true, SHOW_DURATION, Game.GUIInventory));
                    _scaleTask.Finished += (bool isManual) => 
                    {
                        _scaleTask = null;
                    };
                }
                else
                {
                    _scaleTask = new CoroutineTask(GUISingleton.Scale(false, HIDE_DURATION, Game.GUIInventory));
                    _scaleTask.Finished += (bool isManual) => 
                    {
                        Camera.main.depth = 1;
                        Game.GUIBusy = false;
                        _scaleTask = null;
                        Game.GUIInventory.SetActive(false);
                    };
                }
            } 
        }
    } 
}