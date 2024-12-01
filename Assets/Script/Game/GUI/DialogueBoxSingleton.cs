using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class DialogueBoxSingleton : MonoBehaviour
{  
    public static DialogueBoxSingleton Instance { get; private set; }  
 
    private int _current_line = 0;
    private Coroutine scrollCoroutine;
    private CharTalk _entityState;
    private CoroutineTask scaleTask;
    
    [SerializeField] private AudioClip TEXT;
    [SerializeField] private float EASE_SPEED = 0.4f;  
    [SerializeField] private float SHOW_DURATION = 0.5f;  
    [SerializeField] private float HIDE_DURATION = 0.2f;  
      
     
     
    private void Awake()
    {
        Instance = this;
    }
 
    private void Update()
    {
        if (_entityState != null){ 
            
            if (!scaleTask.Running && Vector3.Distance(Game.Player.transform.position, _entityState._esm.transform.position) > 3) { //walk away from npc
                StartCoroutine(HideDialogue());
                if (scrollCoroutine != null) StopCoroutine(scrollCoroutine);
            }
            
            if (Input.GetKeyDown(KeyCode.F))
            { 
                AudioSingleton.PlaySFX(TEXT, 0.2f); //sound effect click 
                
                if (scrollCoroutine != null)
                {
                    StopCoroutine(scrollCoroutine);
                    scrollCoroutine = null;
                    AudioSingleton.StopSFX(GUISingleton._audioSource);
                    Game.DialogueText.text = _entityState._dialogueData.Lines[_current_line];
                }                 
                else if (_current_line < _entityState._dialogueData.Lines.Count - 1)
                {    
                    ++_current_line; 
                    scrollCoroutine = StartCoroutine(GUISingleton.ScrollText(_entityState._dialogueData.Lines[_current_line], Game.DialogueText, TEXT));
                }
                else
                {    
                    StartCoroutine(HideDialogue());
                } 
            }
        }
    }

    public IEnumerator StartDialogue(CharTalk entityState)
    {    
        _entityState = entityState;
        _current_line = 0; 
        Game.DialogueBox.SetActive(true);
        scrollCoroutine = StartCoroutine(GUISingleton.ScrollText(_entityState._dialogueData.Lines[_current_line], Game.DialogueText, TEXT));
        scaleTask = new CoroutineTask(GUISingleton.Scale(true, SHOW_DURATION, Game.DialogueBox, EASE_SPEED)); 

        yield return null; 
    }

    private IEnumerator HideDialogue()
    {
        _entityState.OnEndDialogue();
        _entityState = null;

        scaleTask = new CoroutineTask(GUISingleton.Scale(false, HIDE_DURATION, Game.DialogueBox, EASE_SPEED));

        while (scaleTask.Running)
        {
            yield return null;
        }
        Game.DialogueBox.SetActive(false);
        AudioSingleton.StopSFX(GUISingleton._audioSource);
    }
 
}


 