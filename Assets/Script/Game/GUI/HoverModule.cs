using UnityEngine;
using UnityEngine.EventSystems;

public class HoverModule : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GUI GUI;

    public void OnPointerEnter(PointerEventData eventData)
    {
        GUI.IsHover = true;
        GUIMain.IsHover = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        GUI.IsHover = false;
        GUIMain.IsHover = false;
    }
}