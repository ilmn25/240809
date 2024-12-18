using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class GUI 
{
    private const float ShowDuration = 0.5f;
    private const float HideDuration = 0.2f;
    
    public static bool Active = false;
    
    private static CoroutineTask _showTask; 
    
    public static void Initialize()
    {
        GUICraft.Initialize();
        GUIDialogue.Initialize();
        GUICursor.Initialize();
        GUIStorage.Initialize();
    }
 
    public static void Update()
    {
        Active = Game.GUIInv.activeSelf;
        Camera.main.depth = Active? -1 : 1;
        
        GUIDialogue.Update();
        if (Active)
        {
            if (Control.Inst.ActionPrimary.KeyDown())
                Audio.PlaySFX(Game.PickUpSound);
            
            GUICraft.Update(); 
            GUICursor.Update();
            GUIStorage.Update();
        }

        if (Control.Inst.Inv.KeyDown())  
        {
            if ((_showTask != null && !_showTask.Running) || _showTask == null)
            {
                if (!Game.GUIInv.activeSelf)
                { 
                    Game.GUIInv.SetActive(true);
                    GUIStorage.RefreshCursorSlot();
                    _showTask = new CoroutineTask(GUI.Scale(true, ShowDuration, Game.GUIInv, scale : 0.7f));
                    _showTask.Finished += (bool isManual) => 
                    {
                        _showTask = null;
                    };
                }
                else
                {
                    _showTask = new CoroutineTask(GUI.Scale(false, HideDuration, Game.GUIInv, scale : 0.7f));
                    _showTask.Finished += (bool isManual) => 
                    {
                        _showTask = null;
                        Game.GUIInv.SetActive(false);
                    };
                }
            } 
        }
    }

    public static IEnumerator ScrollText(string line, TextMeshProUGUI textBox, int speed = 75)
    {
        textBox.text = ""; 
    
        foreach (var letter in line.ToCharArray())        
        { 
            textBox.text += letter;
            yield return new WaitForSeconds(1f / speed);
        }
      
    }

    public static IEnumerator Scale(bool show, float duration, GameObject target, float easeSpeed = 0.5f, float scale = 1f)
    {
        Vector3 targetScale = show ? Vector3.one * scale : Vector3.zero;
        Vector3 initialScale = show ? Vector3.zero : Vector3.one * scale;
        float elapsedTime = 0f;
        target.transform.localScale = initialScale;  

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            if (show)
            {
                // if (target.transform.localScale.x > 0.5f) _isInputBlocked = false;
                t = Mathf.SmoothStep(0f, 1f, Mathf.Pow(t, easeSpeed)); // Apply adjustable ease-out effect
            }
            else
            {
                t = Mathf.Lerp(0f, 1f, t); // Linear interpolation for hiding
            }

            target.transform.localScale = Vector3.Lerp(initialScale, targetScale, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        target.transform.localScale = targetScale;
    }
 
}
