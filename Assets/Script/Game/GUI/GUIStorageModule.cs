using UnityEngine;
using UnityEngine.EventSystems;

public class GUIStorageModule : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GUIStorage GUIStorage;
    public void OnPointerEnter(PointerEventData eventData)
    { GUIStorage.IsHover = true;}
 
    public void OnPointerExit(PointerEventData eventData)
    { GUIStorage.IsHover = false;}
}