using UnityEngine;

public class GUI
{ 
    protected float ShowSpeed = 0.5f;
    protected float HideSpeed = 0.2f;
    
    public bool IsHover;
    protected RectTransform Rect;
    public bool IsDrag;
    public Vector2 Position;
    protected GameObject GameObject;
    protected RectTransform ParentRect; 
    private Vector2 _dragOffset;
    private CoroutineTask _scaleTask;
    private bool _showing = true;
    private GameObject _shadow; 

    public void Initialize()
    {
        Rect.anchoredPosition = Position;
        ParentRect = Rect.parent.GetComponent<RectTransform>();
    }
    public void UpdateDrag()
    {
        if (IsHover || IsDrag)
        { 
            if (Control.Inst.ActionPrimary.KeyDown())
            {
                Position = Rect.anchoredPosition;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    ParentRect,
                    Input.mousePosition,
                    Game.GUICamera,
                    out _dragOffset
                );
                IsDrag = true;
            }
                
            if (IsDrag && Control.Inst.ActionPrimary.Key())
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    ParentRect,
                    Input.mousePosition,
                    Game.GUICamera,
                    out Vector2 mousePosition
                );

                Rect.anchoredPosition = mousePosition - _dragOffset + Position;
            }
                
            if (Control.Inst.ActionPrimary.KeyUp())
            {
                IsDrag = false;
            }
        }
    }  
    public void Show(bool isShow, bool instant = false)
    {
        if (isShow)
        {
            if (!_showing)
            {
                _scaleTask?.Stop();
                if (instant)
                    GameObject.transform.localScale = Vector3.one * 0.9f;
                else
                    _scaleTask = new CoroutineTask(GUIMain.Scale(true, ShowSpeed, GameObject, 0.9f));
                _showing = true;
            }
        }
        else
        {
            if (_showing)
            {
                _scaleTask?.Stop();
                _scaleTask = new CoroutineTask(GUIMain.Scale(false, HideSpeed, GameObject, 0));
                _showing = false;
            }
        } 
    }
}