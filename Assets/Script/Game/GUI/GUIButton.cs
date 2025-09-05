using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class GUIButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{ 
    private TextMeshProUGUI _text;
    [NonSerialized] public string Info;
    [NonSerialized] public bool IsHovered;
    private void Start()
    {
        _text = transform.Find("Text").GetComponent<TextMeshProUGUI>();
    } 
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        ScaleSlot(1.1f);
        IsHovered = true;
        if (Info != null) GUIMain.Cursor.Set(_text.text, Info);
    }
 
    public void OnPointerExit(PointerEventData eventData)
    {
        ScaleSlot(1f);
        IsHovered = false;
        GUIMain.Cursor.Set();
    }
    private void ScaleSlot(float scale)
    {
        _text.rectTransform.localScale = Vector3.one * scale; 
    }
}