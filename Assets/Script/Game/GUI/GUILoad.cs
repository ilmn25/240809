using System.Collections;
using UnityEngine;

public class GUILoad : GUI
{
    public static int Target = -1;
    private CoroutineTask _decelerateCoroutine;
    private float _scrollSpeed;

    public GUILoad()
    { 
        GameObject = Main.GUILoadMenu;
        Rect = Main.GUILoadMenu.GetComponent<RectTransform>();
        for (int i = 0; i < Save.Inst.List.Count; i++) AddToList(i);
    } 

    public static void AddToList(int i)
    {
        GameObject obj = Object.Instantiate(Resources.Load<GameObject>("Prefab/GUISave"), Main.GUILoadMenu.transform);
        obj.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, i * -100 + 150, 130);
        obj.GetComponent<GUISave>().ID = i; 
    }
    
    public void Update()
    {
        if (Showing)
        { 
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0) 
                HandleScrollInput(scroll);
        }
        if (Control.Inst.Pause.KeyDown() && Showing)
        {
            Show(false);
            GUIMain.GUIMenu.Show(true);
        }
 
        if (Control.Inst.ActionPrimary.KeyDown() && Target != -1)
        {  
            Scene.SwitchWorld(Save.Inst.List[Target]);
            Show(false); 
        } 
    }
 
    public void HandleScrollInput(float input)
    {
        _scrollSpeed = input * -5000;

        if (_decelerateCoroutine != null && _decelerateCoroutine.Running)
            _decelerateCoroutine.Stop();

        _decelerateCoroutine = new CoroutineTask(Decelerate());
        return;

        IEnumerator Decelerate()
        {
            while (Mathf.Abs(_scrollSpeed) > 0.1f)
            {
                if ((Rect.anchoredPosition.y < 5 && _scrollSpeed < 0) || 
                    (Rect.anchoredPosition.y > 335 && _scrollSpeed > 0)) 
                    _scrollSpeed = 0;
                Rect.anchoredPosition += new Vector2(0, _scrollSpeed * Time.deltaTime);
                _scrollSpeed = Mathf.Lerp(_scrollSpeed, 0, Time.deltaTime * 5);
                yield return null;
            }

            _scrollSpeed = 0;
        }
    }
}