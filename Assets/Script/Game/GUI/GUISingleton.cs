using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class GUISingleton : MonoBehaviour
{
    public static GUISingleton Instance { get; private set; }  
    
    private void Awake()
    {
        Instance = this;
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


    public static IEnumerator Scale(bool show, float duration, GameObject target, float easeSpeed = 0.5f)
    {
        Vector3 targetScale = show ? Vector3.one : Vector3.zero;
        Vector3 initialScale = show ? Vector3.zero : Vector3.one;
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
