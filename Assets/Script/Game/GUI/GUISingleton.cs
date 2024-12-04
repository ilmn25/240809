using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class GUISingleton : MonoBehaviour
{
    public static GUISingleton Instance { get; private set; }  
      

    private CoroutineTask _scaleTask;
    private float SHOW_DURATION = 0.5f;
    private float HIDE_DURATION = 0.2f;
    
    private void Start()
    {
        Instance = this;  
    }
 
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))  
        {
            if (_scaleTask == null || (_scaleTask != null && !_scaleTask.Running))
            {
                if (!Game.GUIInv.activeSelf)
                { 
                    Game.GUIInv.SetActive(true);
                    GUIStorageSingleton.Instance.RefreshCursorSlot();
                    _scaleTask = new CoroutineTask(GUISingleton.Scale(true, SHOW_DURATION, Game.GUIInv, scale : 0.7f));
                    _scaleTask.Finished += (bool isManual) => 
                    {
                        _scaleTask = null;
                    };
                }
                else
                {
                    _scaleTask = new CoroutineTask(GUISingleton.Scale(false, HIDE_DURATION, Game.GUIInv, scale : 0.7f));
                    _scaleTask.Finished += (bool isManual) => 
                    {
                        _scaleTask = null;
                        Game.GUIInv.SetActive(false);
                    };
                }
            } 
        }
        
        
        if (Game.GUIInv.activeSelf)
        { 
            Game.GUIBusy = true;
            Camera.main.depth = -1;
        }
        else
        {
            Game.GUIBusy = false;
            Camera.main.depth = 1;
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
