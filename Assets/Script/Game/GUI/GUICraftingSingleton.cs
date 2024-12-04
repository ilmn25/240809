using System;
using Script.World.Entity.Item;
using TMPro;
using UnityEngine;

public class GUICraftingSingleton : MonoBehaviour
{
    public static GUICraftingSingleton Instance { get; private set; }   
    private string _stringID;
     
    private int SLOT_SIZE = 30;
    private Vector2 MARGIN = new Vector2(10, 10);
    private void Start()
    {
        Instance = this;
        Initialize();
    }

    private void Initialize()
    {
        int count = 1;
        foreach (var recipe in CraftSingleton._craftList)
        {
            GameObject slot = Instantiate(Resources.Load<GameObject>($"prefab/gui_item_slot"), Game.GUIInvCrafting.transform, false);
     
            RectTransform slotRectTransform = slot.GetComponent<RectTransform>();
            slotRectTransform.sizeDelta = new Vector2(SLOT_SIZE, SLOT_SIZE);
            slotRectTransform.anchoredPosition = new Vector2(0, count * (SLOT_SIZE + MARGIN.y));
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

        ItemData itemData = ItemSingleton.GetItem(stringID);
        CraftData craftData = CraftSingleton.GetItem(stringID);
        
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
            CraftSingleton.Instance.CraftItem(_stringID);
            GUICursorSingleton.UpdateCursorSlot();
        } 
    }
}
