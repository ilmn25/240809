using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GUIDialogueSingleton : MonoBehaviour
{  
    public static GUIDialogueSingleton Instance { get; private set; }  
 
    private int _current_line = 0;
    private CoroutineTask scrollTask;
    private CharTalk _entityState;
    private CoroutineTask scaleTask;
    private AudioSource _audioSource;
    
    [SerializeField] private AudioClip _textSFX;
    [SerializeField] private float EASE_SPEED = 0.4f;  
    [SerializeField] private float SHOW_DURATION = 0.5f;  
    [SerializeField] private float HIDE_DURATION = 0.2f;  
      
    public static Action DialogueAction;
     
    private void Awake()
    {
        Instance = this; 
    }
 
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && _entityState == null) DialogueAction?.Invoke();

        if (_entityState != null){ 
            
            if (!scaleTask.Running && Vector3.Distance(Game.Player.transform.position, _entityState._esm.transform.position) > 3) { //walk away from npc
                HideDialogue();
                if (scrollTask.Running) scrollTask.Stop();
            }
            
            if (Input.GetKeyDown(KeyCode.F))
            {
                if (_entityState == null)
                {
                    DialogueAction.Invoke();
                }
                
                AudioSingleton.PlaySFX(_textSFX, 0.2f); //sound effect click 
                
                if (scrollTask.Running)
                {
                    scrollTask.Stop(); 
                    Game.GUIDialogueText.text = _entityState._dialogueData.Lines[_current_line];
                }                 
                else if (_current_line < _entityState._dialogueData.Lines.Count - 1)
                {    
                    ++_current_line; 
                    HandleScroll();
                }
                else if (!scaleTask.Running)
                {
                    HideDialogue();
                } 
            }
        }
    }
    
    private void HandleScroll()
    {
        _audioSource = AudioSingleton.PlaySFX(_textSFX, 0.2f, true);
        
        scrollTask =  new CoroutineTask(GUISingleton.ScrollText(_entityState._dialogueData.Lines[_current_line], Game.GUIDialogueText));
        scrollTask.Finished += (bool isManual) => 
        { 
            AudioSingleton.StopSFX(_audioSource);
            _audioSource = null;
        }; 
    }

    public void StartDialogue(CharTalk entityState)
    {
        if (_entityState != null)
        {
            entityState.OnEndDialogue(); 
            return;
        } 
        
        AudioSingleton.PlaySFX(Game.ChatSound);
        _entityState = entityState;
        _current_line = 0;
        Game.GUIDialogue.SetActive(true);
        HandleScroll(); 
        scaleTask = new CoroutineTask(GUISingleton.Scale(true, SHOW_DURATION, Game.GUIDialogue, EASE_SPEED, 0.9f)); 
    }
 
    private void HideDialogue()
    {
        _entityState.OnEndDialogue(); 
        
        scaleTask = new CoroutineTask(GUISingleton.Scale(false, HIDE_DURATION, Game.GUIDialogue, EASE_SPEED, 0.9f));
        scaleTask.Finished += (bool isManual) => 
        {
            Game.GUIDialogue.SetActive(false);
            _entityState = null;
        }; 
    }
}


 