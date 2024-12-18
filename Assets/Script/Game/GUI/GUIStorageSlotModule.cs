using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GUIInvSlotModule : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Image _image;
    private TextMeshProUGUI _text;
    public int slotNumber;
    
    private void Awake()
    {
        GUIStorage.OnRefreshSlot += OnRefreshSlot;
        _image = transform.Find("image").GetComponent<Image>();
        _text = transform.Find("text").GetComponent<TextMeshProUGUI>();
    }

    public void OnRefreshSlot()
    {
        InvSlot slot = Inventory.Storage[slotNumber];
        if (slot.Stack != 0)
        {
            _image.sprite = Resources.Load<Sprite>($"texture/sprite/{slot.StringID}");
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