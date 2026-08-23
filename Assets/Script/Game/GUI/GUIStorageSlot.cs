using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GUIStorageSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Image _image;
    private Image _panel;
    private Sprite _defaultPanel;
    private TextMeshProUGUI _text;
    public int slotNumber;
    public GUIStorage GUIStorage;
    
    private void Start()
    {
        GUIStorage.OnRefreshSlot += OnRefreshSlot;
        _image = transform.Find("Image").GetComponent<Image>();
        _text = transform.Find("Text").GetComponent<TextMeshProUGUI>();
        _panel = transform.Find("Panel").GetComponent<Image>();
        _defaultPanel = _panel.sprite;
    }

    private void OnRefreshSlot(object sender, EventArgs e)
    {
        if (GUIStorage.Storage == null) return;
        ItemSlot slot = GUIStorage.Storage.List[slotNumber];
        if (slot.Stack != 0)
        {
            _image.sprite =Cache.LoadSprite("Sprite/" + slot.ID);
            _image.color = Color.white;
            _text.text = slot.Stack.ToString();
        }
        else 
        {
            _image.color = Color.clear;
            _text.text = "";
        } 

        // Highlight the self-inventory's selected slot in red (hidden while an
        // item is held on the cursor, since the cursor item takes over as held).
        bool isSelected = ReferenceEquals(GUIStorage, GUIMain.StorageInv) &&
                          slotNumber == GUIStorage.Storage.Key &&
                          GUICursor.Data.isEmpty();
        _panel.sprite = isSelected ? Cache.LoadSprite("GUI/PanelRed") : _defaultPanel;
    }
 
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!GUIMain.Showing || !GUIStorage.Showing) return;
        if (GUIStorage.ScaleTask is { Running: true }) return;
        if (GUIStorage.IsDrag) return;
        if (GUIStorage.Storage.List[slotNumber].Stack != 0)
            Audio.PlaySFX(SfxID.Text);
        GUIStorage.SetInfoPanel(slotNumber);
        ScaleSlot(1.1f);
    }
 
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!GUIMain.Showing || !GUIStorage.Showing) return;
        if (GUIStorage.ScaleTask is { Running: true }) return;
        GUIStorage.SetInfoPanel();
        ScaleSlot(1f);
    }

    private void ScaleSlot(float scale)
    {
        _image.rectTransform.localScale = Vector3.one * scale; 
    }
}