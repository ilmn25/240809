using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GUICraftingSingleton : MonoBehaviour
{
    public static GUICraftingSingleton Instance { get; private set; }   
    private string _stringID;
     
    private RectTransform _craftRect;
    private int SLOT_SIZE = 30;
    private Vector2 MARGIN = new Vector2(10, 10);
    private void Start()
    {
        _craftRect = Game.GUIInvCrafting.GetComponent<RectTransform>();
        Instance = this;
        Initialize();
    }

    private void Initialize()
    {
        int count = 1;
        foreach (var recipe in Craft.Dictionary)
        {
            GameObject slot = Instantiate(Resources.Load<GameObject>($"prefab/gui_item_slot"), Game.GUIInvCrafting.transform, false);
     
            RectTransform slotRectTransform = slot.GetComponent<RectTransform>();
            slotRectTransform.sizeDelta = new Vector2(SLOT_SIZE, SLOT_SIZE);
            slotRectTransform.anchoredPosition = new Vector2(count * (SLOT_SIZE + MARGIN.x), 0);
            slot.AddComponent<GUICraftSlotModule>().Initialize(recipe.Key);
            count++;
        }
    }
    
    public void SetInfoPanel(string stringID = null)
    {
        _stringID = stringID;
        
        if (stringID == null)
        {  
            GUICursorSingleton.SetInfoPanel(); 
            return;
        }

        ItemData itemData = Item.GetItem(stringID);
        CraftData craftData = Craft.GetItem(stringID);
        
        string text = itemData.Name + " (" + craftData.Stack + ")\n";
        foreach (var ingredient in craftData.Ingredients)
        {
            text += $"{ingredient.Key} ({ingredient.Value})\n";
        }
        text += itemData.Description + "\n";
        
        GUICursorSingleton.SetInfoPanel(text);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && _stringID != null)
        {
            Craft.CraftItem(_stringID);
            GUICursorSingleton.UpdateCursorSlot();
        } 
    }

    private Coroutine decelerateCoroutine;
    private float scrollSpeed;
    
    public void HandleScrollInput(float input)
    {
        scrollSpeed = input * 10000;

        if (decelerateCoroutine != null)
        {
            StopCoroutine(decelerateCoroutine);
        }

        decelerateCoroutine = StartCoroutine(Decelerate());
    }

    private IEnumerator Decelerate()
    {
        while (Mathf.Abs(scrollSpeed) > 0.1f)
        {
            _craftRect.anchoredPosition += new Vector2(scrollSpeed * Time.deltaTime, 0);
            scrollSpeed = Mathf.Lerp(scrollSpeed, 0, Time.deltaTime * 5);
            yield return null;
        }
        scrollSpeed = 0;
    }
}
