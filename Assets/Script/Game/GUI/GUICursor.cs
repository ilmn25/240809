using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GUICursor : GUI
{
    public static ItemSlot Data = new ItemSlot();

    private static TextMeshProUGUI _cursorInfoText; 
    private static TextMeshProUGUI _cursorSlotText;
    private static Image _cursorSlotImage;
    
    public new void Initialize()
    {
        ShowSpeed = 0.25f;
        HideSpeed = 0.1f;
        
        Rect = Game.GUICursor.GetComponent<RectTransform>();
        GameObject = Game.GUICursorInfo;
        base.Initialize();        
        _cursorInfoText = Game.GUICursorInfo.transform.Find("text").GetComponent<TextMeshProUGUI>(); 
        _cursorSlotText = Game.GUICursorSlot.transform.Find("text").GetComponent<TextMeshProUGUI>();
        _cursorSlotImage = Game.GUICursorSlot.transform.Find("image").GetComponent<Image>();
    }

    public void Update()
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(ParentRect, Input.mousePosition,  
            Game.GUICamera,out Vector2 mousePosition);
        Rect.anchoredPosition = mousePosition;
    }

    public void Set(string info = null)
    {
        if (info == null)
        {
            Show(false);
            return;
        }
        Show(true);
        _cursorInfoText.text = info;
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
