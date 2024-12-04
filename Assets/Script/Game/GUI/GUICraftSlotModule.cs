using Script.World.Entity.Item;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GUICraftSlotModule : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
        private Image _image;
        private TextMeshProUGUI _text;
        public string StringID;
        public CraftData CraftData;
         
        public void Initialize(string stringID)
        {
                StringID = stringID;
                CraftData = CraftSingleton.GetItem(stringID);
                
                _image = transform.Find("image").GetComponent<Image>();
                _text = transform.Find("text").GetComponent<TextMeshProUGUI>();
                
                _image.sprite = Resources.Load<Sprite>($"texture/sprite/{StringID}");
                _image.color = Color.white;
                _text.text = CraftData.Stack.ToString();
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
                GUICraftingSingleton.Instance.SetInfoPanel(StringID);
                ScaleSlot(1.1f);
        }
 
        public void OnPointerExit(PointerEventData eventData)
        {
                GUICraftingSingleton.Instance.SetInfoPanel();
                ScaleSlot(1f);
        }
        
        private void ScaleSlot(float scale)
        {
                _image.rectTransform.localScale = Vector3.one * scale; 
        }
}