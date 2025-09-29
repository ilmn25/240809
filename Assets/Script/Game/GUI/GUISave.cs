using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class GUISave : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{ 
    private TextMeshProUGUI _text;
    private Image _image;
    [NonSerialized] public int ID;
    private void Start()
    {
        _text = transform.Find("Text").GetComponent<TextMeshProUGUI>();
        _image = transform.Find("Image").GetComponent<Image>();
        Refresh();
    } 
    private void Refresh()
    {
        if (ID > Save.Inst.List.Count)
        {
            Destroy(this);
            return;
        }
        SaveData save = Save.Inst.List[ID]; 
        _text.text = ID + ". " + save.current + "\nDay " + save.day + ", " + save.weather;
        Sprite sprite = Helper.LoadImage(Save.Inst.List[ID].Path + "Preview");
        if (sprite)
        {
            _image.enabled = true;
            _image.sprite = sprite;
        }
        else _image.enabled = false;
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        ScaleSlot(0.94f);
        GUILoad.Target = ID;
    }
 
    public void OnPointerExit(PointerEventData eventData)
    {
        ScaleSlot(0.9f);
        GUILoad.Target = -1;
        GUIMain.Cursor.Set();
    }
    private void ScaleSlot(float scale)
    {
        transform.localScale = Vector3.one * scale; 
    }
}