using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GUICraftSlotModule : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
        public string stringID;
        private Craft _craftData;
        
        private Image _image;
        private TextMeshProUGUI _text;
         
        public void Initialize(string stringID)
        {
                this.stringID = stringID;
                _craftData = Craft.GetItem(stringID);
                
                _image = transform.Find("image").GetComponent<Image>();
                _text = transform.Find("text").GetComponent<TextMeshProUGUI>();
                
                _image.sprite = Resources.Load<Sprite>($"texture/sprite/{this.stringID}");
                _image.color = Color.white;
                _text.text = _craftData.Stack.ToString();
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
                GUICraft.SetInfoPanel(stringID);
                ScaleSlot(1.1f);
        }
 
        public void OnPointerExit(PointerEventData eventData)
        {
                GUICraft.SetInfoPanel();
                ScaleSlot(1f);
        }
        
        private void ScaleSlot(float scale)
        {
                _image.rectTransform.localScale = Vector3.one * scale; 
        }
}