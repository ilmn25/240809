using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GUICursor : GUI
{
    public static ItemSlot Data = new ItemSlot();

    private static TextMeshProUGUI _infoSlotText;
    private static TextMeshProUGUI _cursorSlotText;
    private static Image _cursorSlotImage;
    
    public new void Initialize()
    {
        ShowSpeed = 0.25f;
        HideSpeed = 0.1f;
        
        Rect = Game.GUICursor.GetComponent<RectTransform>();
        GameObject = Game.GUICursorInfo;
        base.Initialize();        
        _infoSlotText = Game.GUICursorInfo.transform.Find("Info").GetComponent<TextMeshProUGUI>(); 
        Text = Game.GUICursorInfo.transform.Find("Text").GetComponent<TextMeshProUGUI>(); 
        _cursorSlotText = Game.GUICursorSlot.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        _cursorSlotImage = Game.GUICursorSlot.transform.Find("Image").GetComponent<Image>();
    }

    public void Update()
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(ParentRect, Input.mousePosition,  
            Game.GUICamera,out Vector2 mousePosition);
        Rect.anchoredPosition = mousePosition;
    }

    public void Set(ItemSlot item = null, bool ingredient = false)
    {
        if (item == null)
        {
            Show(false);
            return;
        }
        Text.text = item.Info.Name;
        _infoSlotText.text = item.ToString(ingredient);
        Show(true); 
    } 
    
    public static void UpdateCursorSlot()
    { 
        if (Data.Stack == 0)
        {
            Game.GUICursorSlot.SetActive(false);
        }
        else
        {
            Game.GUICursorSlot.SetActive(true);
            _cursorSlotImage.sprite = Resources.Load<Sprite>($"texture/sprite/{Data.ID}");
            _cursorSlotText.text = Data.Stack.ToString();
        } 
    } 
}
