using System.Collections;
using TMPro;
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
    public CoroutineTask ScaleTask;
    public bool Showing = true;
    private GameObject _shadow; 

    protected TextMeshProUGUI Text;
    private CoroutineTask _textScrollTask;
    
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
            if (!Showing)
            {
                ScaleTask?.Stop();
                if (instant)
                    GameObject.transform.localScale = Vector3.one * 0.9f;
                else
                    ScaleTask = new CoroutineTask(GUIMain.Scale(true, ShowSpeed, GameObject, 0.9f)); 
                Showing = true;
            }  
            _textScrollTask?.Stop();
            new CoroutineTask(Delay());
        }
        else
        {
            if (Showing)
            {
                ScaleTask?.Stop();
                ScaleTask = new CoroutineTask(GUIMain.Scale(false, HideSpeed, GameObject, 0));
                ScaleTask.Finished += (bool isManual) => 
                { 
                    _textScrollTask?.Stop();
                }; 
                Showing = false;
            }
        } 
        return;
        IEnumerator Delay()
        {
            yield return null;
            _textScrollTask = TextScroller.HandleScroll(Text);
        }
    }
     
     
}