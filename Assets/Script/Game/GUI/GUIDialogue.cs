using System;
using UnityEngine;

public class GUIDialogue 
{  
    private const float EaseSpeed = 0.4f;
    private const float ShowDuration = 0.5f;
    private const float HideDuration = 0.2f;

    private static int _currentLine = 0;
    private static CoroutineTask _scrollTask;
    public static Dialogue Dialogue;
    public static bool Showing = true;
    private static CoroutineTask _scaleTask; 
    private static AudioSource _audioSource;

    public static void Show(bool isShow)
    {
        if (isShow)
        {
            if (!Showing)
            {
                Showing = true;
                _currentLine = 0; 
                HandleScroll(); 
                new CoroutineTask(GUIMain.Slide(true, 0.2f, Game.GUIImage, 
                    new Vector3(220, -95, 203), EaseSpeed));
                _scaleTask?.Stop();
                _scaleTask = new CoroutineTask(GUIMain.Scale(true, ShowDuration, Game.GUIDialogue, 
                    0.9f, EaseSpeed)); 
                Game.GUIDialogue.SetActive(true);
            }
        }
        else
        {
            if (Showing)
            {
                Showing = false;
                new CoroutineTask(GUIMain.Slide(false, 0.1f, Game.GUIImage, 
                    new Vector3(450, -95, 203), EaseSpeed));
                _scaleTask?.Stop();
                _scaleTask = new CoroutineTask(GUIMain.Scale(false, HideDuration, Game.GUIDialogue, 
                    0, EaseSpeed));
                _scaleTask.Finished += (bool isManual) =>
                {
                    if (_scrollTask != null && _scrollTask.Running) _scrollTask.Stop();
                    Game.GUIDialogue.SetActive(false);
                }; 
            }
        }
    }
    public static void Update()
    { 
        if (Showing){  
            
            if (Control.Inst.ActionSecondary.KeyDown() || Control.Inst.ActionSecondaryNear.KeyDown())
            { 
                Audio.PlaySFX("text", 0.2f); 
                
                if (_scrollTask.Running)
                {
                    _scrollTask.Stop(); 
                    Game.GUIDialogueText.text = Dialogue.Lines[_currentLine];
                }                 
                else if (_currentLine < Dialogue.Lines.Count - 1)
                {    
                    ++_currentLine; 
                    HandleScroll();
                }
                else
                {
                    Show(false);
                }
            }
        }
    }
    
    private static void HandleScroll()
    {
        _audioSource = Audio.PlaySFX("text", 0.2f, true);
        
        _scrollTask =  new CoroutineTask(GUIMain.ScrollText(Dialogue.Lines[_currentLine], Game.GUIDialogueText));
        _scrollTask.Finished += (bool isManual) => 
        { 
            Audio.StopSFX(_audioSource);
            _audioSource = null;
        }; 
    }
 
  
}


 