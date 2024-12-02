using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GUIInventorySingleton : MonoBehaviour
{
    private GameObject inventory; 
    private int slotSize = 25;
    private Vector2 offset = new Vector2(-160, 30);
    private Vector2 padding = new Vector2(5, 5);
    private Dictionary<int, Image> slotImages = new Dictionary<int, Image>();

    private void Start()
    {
        inventory = Game.GUIInventory;
        Camera.main.depth = 0;
        CreateSlots();
    }

    private void CreateSlots()
    {
        int totalSlots = PlayerInventorySingleton.INVENTORY_SLOT_AMOUNT * PlayerInventorySingleton.INVENTORY_ROW_AMOUNT;
        for (int i = 0; i < totalSlots; i++)
        {
            GameObject newSlot = new GameObject("Slot");
            Image slotImage = newSlot.AddComponent<Image>(); // Add Image component
            newSlot.transform.SetParent(inventory.transform, false);

            // Set the position in the grid
            int row = i / PlayerInventorySingleton.INVENTORY_SLOT_AMOUNT;
            int column = i % PlayerInventorySingleton.INVENTORY_SLOT_AMOUNT;

            RectTransform slotRectTransform = newSlot.GetComponent<RectTransform>();
            slotRectTransform.sizeDelta = new Vector2(slotSize, slotSize);
            slotRectTransform.anchoredPosition = new Vector2(
                column * (slotSize + padding.x) + offset.x, 
                -row * (slotSize + padding.y) + offset.y
            );

            // Add SlotScaler component for hover effects
            newSlot.AddComponent<GUIItemSlot>();

            slotImages.Add(PlayerInventorySingleton.CalculateKey(row, column), slotImage);
        }
    }

    public void UpdateSlots()
    {
        foreach (var slot in slotImages)
        {
            int key = slot.Key;

            if (PlayerInventorySingleton._playerInventory.TryGetValue(key, out InvSlotData slotData))
            {
                // Load and set the sprite based on the stringID
                Sprite itemSprite = Resources.Load<Sprite>($"texture/sprite/{slotData.StringID}");
                slot.Value.sprite = itemSprite;
                slot.Value.color = Color.white; // Ensure the slot is visible
            }
            else
            {
                // Clear the slot if no item is present
                slot.Value.sprite = null;
                slot.Value.color = Color.clear; // Make slot transparent to indicate empty
            }
        }
    }

    private CoroutineTask scaleTask;
    [SerializeField] private float SHOW_DURATION = 0.5f;  
    [SerializeField] private float HIDE_DURATION = 0.2f;  
    private void Update()
    {
        // Call UpdateSlots() whenever the inventory changes
        if (Input.GetKeyDown(KeyCode.T)) // Example key to trigger update
        {
            if (scaleTask == null || (scaleTask != null && !scaleTask.Running))
            {
                if (!inventory.activeSelf)
                {
                    inventory.SetActive(true);
                    UpdateSlots();
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
                        scaleTask = null;
                        inventory.SetActive(false);
                    };
                }
            } 
        }
    } 
}