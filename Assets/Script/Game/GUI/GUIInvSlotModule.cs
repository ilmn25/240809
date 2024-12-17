using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GUIInvSlotModule : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Image _image;
    private TextMeshProUGUI _text;
    public int SlotNumber;
    
    private void Awake()
    {
        GUIStorageSingleton.OnRefreshSlot += OnRefreshSlot;
        _image = transform.Find("image").GetComponent<Image>();
        _text = transform.Find("text").GetComponent<TextMeshProUGUI>();
    }

    public void OnRefreshSlot()
    {
        InvSlotData slotData = InventorySingleton.PlayerInventory[SlotNumber];
        if (slotData.Stack != 0)
        {
            _image.sprite = Resources.Load<Sprite>($"texture/sprite/{slotData.StringID}");
            _image.color = Color.white;
            _text.text = slotData.Stack.ToString();
        }
        else 
        {
            _image.color = Color.clear;
            _text.text = "";
        } 
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        GUIStorageSingleton.Instance.SetInfoPanel(SlotNumber);
        ScaleSlot(1.1f);
    }
 
    public void OnPointerExit(PointerEventData eventData)
    {
        GUIStorageSingleton.Instance.SetInfoPanel();
        ScaleSlot(1f);
    }

    private void ScaleSlot(float scale)
    {
        _image.rectTransform.localScale = Vector3.one * scale; 
    }
}