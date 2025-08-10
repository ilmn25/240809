using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GUIStorageSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Image _image;
    private TextMeshProUGUI _text;
    public int slotNumber;
    public GUIStorage GUIStorage;
    
    private void Awake()
    {
        GUIStorage.OnRefreshSlot += OnRefreshSlot;
        _image = transform.Find("image").GetComponent<Image>();
        _text = transform.Find("text").GetComponent<TextMeshProUGUI>();
    }

    private void OnRefreshSlot(object sender, EventArgs e)
    {
        ItemSlot slot = GUIStorage.Storage.List[slotNumber];
        if (slot.Stack != 0)
        {
            _image.sprite =Cache.LoadSprite("sprite/" + slot.StringID);
            _image.color = Color.white;
            _text.text = slot.Stack.ToString();
        }
        else 
        {
            _image.color = Color.clear;
            _text.text = "";
        } 
    }
 
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (GUIStorage.IsDrag) return;
            GUIStorage.SetInfoPanel(slotNumber);
            ScaleSlot(1.1f);
    }
 
    public void OnPointerExit(PointerEventData eventData)
    {
        GUIStorage.SetInfoPanel();
        ScaleSlot(1f);
    }

    private void ScaleSlot(float scale)
    {
        _image.rectTransform.localScale = Vector3.one * scale; 
    }
}