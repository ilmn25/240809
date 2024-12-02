using Script.World.Map;
using UnityEngine;
using UnityEngine.EventSystems;

public class GUIItemSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Vector3 _originalScale;

    private void Start()
    {
        gameObject.layer = Game.UILayerIndex;
        Physics.IgnoreLayerCollision(Game.UILayerIndex, Game.MapLayerIndex);
        Physics.IgnoreLayerCollision(Game.UILayerIndex, Game.EntityLayerIndex);
        _originalScale = transform.localScale;
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
        transform.localScale = _originalScale * scale;
    }
}