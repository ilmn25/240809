using System;
using UnityEngine;

public class GUIDialogue 
{  
    private const float EaseSpeed = 0.4f;
    private const float ShowDuration = 0.5f;
    private const float HideDuration = 0.2f;

    
    private static int _currentLine = 0;
    private static CoroutineTask _scrollTask;
    private static CharTalk _entityState;
    private static CoroutineTask _scaleTask;
    private static CoroutineTask _slideTask;
    private static AudioSource _audioSource;
      
    public static void Update()
    { 
        if (_entityState != null){ 
            if (!_scaleTask.Running && Utility.SquaredDistance(Game.Player.transform.position, _entityState.Machine.transform.position) > 5*5) { //walk away from npc
                HideDialogue();
                if (_scrollTask.Running) _scrollTask.Stop();
            }
            
            if (Control.Inst.ActionSecondary.KeyDown() || Control.Inst.ActionSecondaryNear.KeyDown())
            { 
                Audio.PlaySFX("text", 0.2f); 
                
                if (_scrollTask.Running)
                {
                    _scrollTask.Stop(); 
                    Game.GUIDialogueText.text = _entityState.Dialogue.Lines[_currentLine];
                }                 
                else if (_currentLine < _entityState.Dialogue.Lines.Count - 1)
                {    
                    ++_currentLine; 
                    HandleScroll();
                }
                else if (!_scaleTask.Running)
                {
                    HideDialogue();
                } 
            }
        }
    }
    
    private static void HandleScroll()
    {
        _audioSource = Audio.PlaySFX("text", 0.2f, true);
        
        _scrollTask =  new CoroutineTask(GUIMain.ScrollText(_entityState.Dialogue.Lines[_currentLine], Game.GUIDialogueText));
        _scrollTask.Finished += (bool isManual) => 
        { 
            Audio.StopSFX(_audioSource);
            _audioSource = null;
        }; 
    }

    public static void StartDialogue(CharTalk entityState)
    {
        if (_entityState != null)
        {
            entityState.OnEndDialogue(); 
            return;
        }

        Audio.PlaySFX("chat");
        _entityState = entityState;
        _currentLine = 0;
        Game.GUIDialogue.SetActive(true);
        Game.GUIImage.SetActive(true);
        HandleScroll(); 
        _slideTask = new CoroutineTask(GUIMain.Slide(true, 0.2f, Game.GUIImage, 
            new Vector3(220, -95, 203), EaseSpeed));
        _scaleTask = new CoroutineTask(GUIMain.Scale(true, ShowDuration, Game.GUIDialogue, 
            0.9f, EaseSpeed)); 
    }
 
    private static void HideDialogue()
    { 
        _slideTask = new CoroutineTask(GUIMain.Slide(false, 0.1f, Game.GUIImage, 
            new Vector3(450, -95, 203), EaseSpeed));
        _scaleTask = new CoroutineTask(GUIMain.Scale(false, HideDuration, Game.GUIDialogue, 
            0, EaseSpeed));  
        _scaleTask.Finished += (bool isManual) => 
        {
            _entityState.OnEndDialogue(); 
            Game.GUIDialogue.SetActive(false);
            Game.GUIImage.SetActive(false);
            _entityState = null;
        }; 
    }
}


 