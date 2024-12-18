using System;
using UnityEngine;

public class GUIDialogue 
{  
    private const float EaseSpeed = 0.4f;
    private const float ShowDuration = 0.5f;
    private const float HideDuration = 0.2f;

    private static AudioClip _textSfx;
    
    private static int _currentLine = 0;
    private static CoroutineTask _scrollTask;
    private static CharTalk _entityState;
    private static CoroutineTask _scaleTask;
    private static AudioSource _audioSource;
     
    public static void Initialize()
    {
        _textSfx = Resources.Load<AudioClip>("audio/sfx/text");
    }
 
    public static void Update()
    { 
        if (_entityState != null){ 
            if (!_scaleTask.Running && Utility.SquaredDistance(Game.Player.transform.position, _entityState.Machine.transform.position) > 5*5) { //walk away from npc
                HideDialogue();
                if (_scrollTask.Running) _scrollTask.Stop();
            }
            
            if (Control.Inst.ActionSecondary.KeyDown() || Control.Inst.ActionSecondaryNear.KeyDown())
            { 
                Audio.PlaySFX(_textSfx, 0.2f); //sound effect click 
                
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
        _audioSource = Audio.PlaySFX(_textSfx, 0.2f, true);
        
        _scrollTask =  new CoroutineTask(GUI.ScrollText(_entityState.Dialogue.Lines[_currentLine], Game.GUIDialogueText));
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

        Audio.PlaySFX(Game.ChatSound);
        _entityState = entityState;
        _currentLine = 0;
        Game.GUIDialogue.SetActive(true);
        HandleScroll(); 
        _scaleTask = new CoroutineTask(GUI.Scale(true, ShowDuration, Game.GUIDialogue, EaseSpeed, 0.9f)); 
    }
 
    private static void HideDialogue()
    { 
        _scaleTask = new CoroutineTask(GUI.Scale(false, HideDuration, Game.GUIDialogue, EaseSpeed, 0.9f));
        _scaleTask.Finished += (bool isManual) => 
        {
            _entityState.OnEndDialogue(); 
            Game.GUIDialogue.SetActive(false);
            _entityState = null;
        }; 
    }
}


 