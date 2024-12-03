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
            return;
        }

        InvSlotData slotData = PlayerInventorySingleton._playerInventory[_currentSlotKey];
        if (slotData.Stack != 0)
        {
            Game.GUIInventoryCursorInfo.SetActive(true);
            _cursorInfoText.text = slotData.StringID + " (" + slotData.Stack + ")\n" + 
                                   ItemLoadSingleton.GetItem(slotData.StringID).Description + "\n" +
                                   slotData.Modifier;
        } 
    }
    
    private InvSlotData _cursorSlot = new InvSlotData();

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
    
    private void HandleInput()
    {
        if (_currentSlotKey == -1) return; 
  
        if (Input.GetMouseButtonDown(0))
        {
            if (_cursorSlot.isEmpty())
            {
                _cursorSlot.Add(PlayerInventorySingleton._playerInventory[_currentSlotKey]);
            }
            else if (PlayerInventorySingleton._playerInventory[_currentSlotKey].isSame(_cursorSlot))
            {
                PlayerInventorySingleton._playerInventory[_currentSlotKey].Add(_cursorSlot);
            } 
            else
            {
                (PlayerInventorySingleton._playerInventory[_currentSlotKey], _cursorSlot) = 
                    (_cursorSlot, PlayerInventorySingleton._playerInventory[_currentSlotKey]);
            } 
            UpdateCursorSlot();
            Refresh();
        }
        else if (Input.GetMouseButtonDown(1))
        {
            InvSlotData invSlot = PlayerInventorySingleton._playerInventory[_currentSlotKey];
            if (!invSlot.isEmpty())
            {
                if (_cursorSlot.isEmpty() || invSlot.isSame(_cursorSlot))
                {
                    if (Input.GetKey(KeyCode.LeftShift))
                        _cursorSlot.Add(invSlot, invSlot.Stack/2);
                    else 
                        _cursorSlot.Add(invSlot, 1);
                        
                    UpdateCursorSlot();
                    Refresh();
                }
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