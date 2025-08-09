using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GUICursor 
{
    public static InvSlot Data = new InvSlot();

    private static TextMeshProUGUI _cursorInfoText; 
    private static TextMeshProUGUI _cursorSlotText;
    private static Image _cursorSlotImage;

    private static RectTransform _parentRect;
    private static RectTransform _cursorRect;
    
    public static void Initialize()
    {
        _cursorRect = Game.GUICursor.GetComponent<RectTransform>();
        _parentRect = _cursorRect.parent.GetComponent<RectTransform>();
        
        _cursorInfoText = Game.GUICursorInfo.transform.Find("text").GetComponent<TextMeshProUGUI>(); 
        _cursorSlotText = Game.GUICursorSlot.transform.Find("text").GetComponent<TextMeshProUGUI>();
        _cursorSlotImage = Game.GUICursorSlot.transform.Find("image").GetComponent<Image>();
    }

    public static void Update()
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_parentRect, Input.mousePosition,  Game.GUICamera,out Vector2 mousePosition);
        _cursorRect.anchoredPosition = mousePosition;
    }

    public static void SetInfoPanel(string info = null)
    {
        if (info == null)
        {
            Game.GUICursorInfo.SetActive(false);
            return;
        }
        Game.GUICursorInfo.SetActive(true);
        _cursorInfoText.text = info;
    }
    
    public static void UpdateCursorSlot()
    {
        GUI.RefreshStorage();
        if (Data.Stack == 0)
        {
            Game.GUICursorSlot.SetActive(false);
        }
        else
        {
            Game.GUICursorSlot.SetActive(true);
            _cursorSlotImage.sprite = Resources.Load<Sprite>($"texture/sprite/{GUICursor.Data.StringID}");
            _cursorSlotText.text = Data.Stack.ToString();
        } 
    } 
}
