using UnityEngine;

public class FPSLogger : MonoBehaviour
{
    private float fps = 0.0f;
    private int frameCount = 0;
    private float elapsedTime = 0.0f;
    
    private PlayerInfo _playerInfo;


    void Update()
    {
        frameCount++;
        elapsedTime += Time.unscaledDeltaTime;

        if (elapsedTime >= 1.0f)
        {
            fps = frameCount / elapsedTime;
            frameCount = 0;
            elapsedTime = 0.0f;
        }
    }

    void OnGUI()
    {
        // Create a style for the text
        GUIStyle style = new GUIStyle();
        style.fontSize = 24;
        style.normal.textColor = Color.white;

        // Display FPS on the screen
        UnityEngine.GUI.Label(new Rect(10, 10, 100, 20), 
            "FPS: " + Mathf.Ceil(fps), style);
    }
}
