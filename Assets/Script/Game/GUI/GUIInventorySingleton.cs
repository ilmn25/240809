using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GUIInventorySingleton : MonoBehaviour
{
    public static GUIInventorySingleton Instance { get; private set; }  
    
    private GameObject inventory; 
    private int slotSize = 30;
    private Vector2 offset = new Vector2(-160, 30);
    private Vector2 padding = new Vector2(5, 5);
    private Dictionary<int, GUIItemSlotModule> _itemSlotMap = new Dictionary<int, GUIItemSlotModule>();

    private void Start()
    {
        Instance = this;
         
        inventory = Game.GUIInventory; 
        Initialize();
    }

    private void Initialize()
    {
        int totalSlots = PlayerInventorySingleton.INVENTORY_SLOT_AMOUNT * PlayerInventorySingleton.INVENTORY_ROW_AMOUNT;
        for (int i = 0; i < totalSlots; i++)
        {
            GameObject newSlot = Instantiate(Resources.Load<GameObject>($"prefab/gui_item_slot"), inventory.transform, false);

            // Set the position in the grid
            int row = i / PlayerInventorySingleton.INVENTORY_SLOT_AMOUNT;
            int column = i % PlayerInventorySingleton.INVENTORY_SLOT_AMOUNT;

            RectTransform slotRectTransform = newSlot.GetComponent<RectTransform>();
            slotRectTransform.sizeDelta = new Vector2(slotSize, slotSize);
            slotRectTransform.anchoredPosition = new Vector2(
                column * (slotSize + padding.x) + offset.x, 
                -row * (slotSize + padding.y) + offset.y
            );
 
            _itemSlotMap.Add(PlayerInventorySingleton.CalculateKey(row, column), newSlot.GetComponent<GUIItemSlotModule>());
        }
    }

    public void Refresh()
    {
        foreach (var slot in _itemSlotMap)
        {
            if (PlayerInventorySingleton._playerInventory.TryGetValue(slot.Key, out InvSlotData slotData))
                slot.Value.SetItem(slotData);
            else
                slot.Value.SetItem();
        }
    }

    private CoroutineTask scaleTask;
    [SerializeField] private float SHOW_DURATION = 0.5f;  
    [SerializeField] private float HIDE_DURATION = 0.2f;  
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))  
        {
            if (scaleTask == null || (scaleTask != null && !scaleTask.Running))
            {
                if (!inventory.activeSelf)
                {
                    Camera.main.depth = -1;
                    Game.GUIBusy = true;
                    inventory.SetActive(true);
                    Refresh();
                    scaleTask = new CoroutineTask(GUISingleton.Scale(true, SHOW_DURATION, inventory));
                    scaleTask.Finished += (bool isManual) => 
                    {
                        scaleTask = null;
                    };
                }
                else
                {
                    scaleTask = new CoroutineTask(GUISingleton.Scale(false, HIDE_DURATION, inventory));
                    scaleTask.Finished += (bool isManual) => 
                    {
                        Camera.main.depth = 1;
                        Game.GUIBusy = false;
                        scaleTask = null;
                        inventory.SetActive(false);
                    };
                }
            } 
        }
    } 
}