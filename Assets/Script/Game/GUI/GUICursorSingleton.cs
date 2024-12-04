using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GUICursorSingleton : MonoBehaviour
{
    public static GUICursorSingleton Instance { get; private set; }
    public static InvSlotData _cursorSlot = new InvSlotData();

    private static TextMeshProUGUI _cursorInfoText; 
    private static TextMeshProUGUI _cursorSlotText;
    private static Image _cursorSlotImage;

    private RectTransform _parentRect;
    private RectTransform _cursorRect;
    private void Start()
    {
        Instance = this;
        _cursorRect = Game.GUICursor.GetComponent<RectTransform>();
        _parentRect = _cursorRect.parent.GetComponent<RectTransform>();
        
        _cursorInfoText = Game.GUICursorInfo.transform.Find("text").GetComponent<TextMeshProUGUI>(); 
        _cursorSlotText = Game.GUICursorSlot.transform.Find("text").GetComponent<TextMeshProUGUI>();
        _cursorSlotImage = Game.GUICursorSlot.transform.Find("image").GetComponent<Image>();
    }

    private void Update()
    {
        if (Game.GUIBusy)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_parentRect, Input.mousePosition,  Game.GUICamera,out Vector2 mousePosition);
            _cursorRect.anchoredPosition = mousePosition;
        } 
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
        if (_cursorSlot.Stack == 0)
        {
            Game.GUICursorSlot.SetActive(false);
        }
        else
        {
            Game.GUICursorSlot.SetActive(true);
            _cursorSlotImage.sprite = Resources.Load<Sprite>($"texture/sprite/{GUICursorSingleton._cursorSlot.StringID}");
            _cursorSlotText.text = _cursorSlot.Stack.ToString();
        } 
    } 
}
