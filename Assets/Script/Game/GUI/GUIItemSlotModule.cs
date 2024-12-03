using Script.World.Map;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GUIItemSlotModule : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private Image _image;
    [SerializeField]
    private TextMeshProUGUI _text;
    public int SlotNumber;
    
    private void Awake()
    {
        GUIInventorySingleton.OnRefreshSlot += OnRefreshSlot;
        gameObject.layer = Game.UILayerIndex;
    }

    public void OnRefreshSlot()
    {
        if (PlayerInventorySingleton._playerInventory.TryGetValue(SlotNumber, out InvSlotData slotData))
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
        GUIInventorySingleton.Instance.SetInfoPanel(SlotNumber);
        ScaleSlot(1.1f);
    }
 
    public void OnPointerExit(PointerEventData eventData)
    {
        GUIInventorySingleton.Instance.SetInfoPanel();
        ScaleSlot(1f);
    }

    private void ScaleSlot(float scale)
    {
        _image.rectTransform.localScale = Vector3.one * scale; 
    }
}