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
    
    private void Start()
    {
        gameObject.layer = Game.UILayerIndex;
    }

    public void SetItem(InvSlotData slotData = null)
    {
        if (slotData == null)
        {
            _image.color = Color.clear;
            _text.text = "";
            return;
        }
        _image.sprite = Resources.Load<Sprite>($"texture/sprite/{slotData.StringID}");
        _image.color = Color.white;
        _text.text = slotData.Stack.ToString();
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        ScaleSlot(1.5f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ScaleSlot(1f);
    }

    private void ScaleSlot(float scale)
    {
        _image.rectTransform.localScale = Vector3.one * scale; 
    }
}