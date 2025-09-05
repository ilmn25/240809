using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GUISave : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{ 
    private TextMeshProUGUI _text;
    private Image _image;
    [NonSerialized] public string ID;
    private void Start()
    {
        _text = transform.Find("Text").GetComponent<TextMeshProUGUI>();
        _image = transform.Find("Image").GetComponent<Image>();
        SaveData save = Save.Inst.List[ID];
        _text.text = ID + "\n" + save.current + "\nDay " + save.day + ", " + save.weather;
        _image.sprite = Helper.LoadImage(ID + "\\Preview");
    } 
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        ScaleSlot(0.94f);
        GUILoad.Target = ID;
    }
 
    public void OnPointerExit(PointerEventData eventData)
    {
        ScaleSlot(0.9f);
        GUILoad.Target = null;
        GUIMain.Cursor.Set();
    }
    private void ScaleSlot(float scale)
    {
        transform.localScale = Vector3.one * scale; 
    }
}