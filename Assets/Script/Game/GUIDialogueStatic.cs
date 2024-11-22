using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class GUIDialogueStatic : MonoBehaviour
{  
    public static GUIDialogueStatic Instance { get; private set; }  
    // C_gamestate
    public event Action OnShowDialog;
    public event Action OnHideDialog;
 
    private AudioSource _audioSource;
    
    private Vector3 _position;
    private DialogueData _currentDialogueData;
    private int _current_line = 0;
    private Coroutine showDialogueCoroutine;
    private bool _isTyping = false;
    private bool _isInputBlocked = true;
    private int _skip = 1; 
    // 0 = _skip now 1 = unlocked 2 = locked 

    [SerializeField] private AudioClip TEXT;
    [SerializeField] private float TEXT_SPEED = 75; 
    [SerializeField] private float EASE_SPEED = 0.4f; // Adjust this value to control the ease-out speed
    [SerializeField] private float SHOW_DURATION = 0.5f; // Duration for showing animation
    [SerializeField] private float HIDE_DURATION = 0.2f; // Duration for hiding animation 
     

    public void PlayDialogue(DialogueData dialogueData, Vector3 position)
    {
        if (dialogueData.Lines.Count() == 0)
        {
            Lib.Log("Dialogue is empty");
            return;
        }
        StartCoroutine(Instance.HandleDialogue(dialogueData, transform.position));
    }

    public void EndDialogue()
    {
        StartCoroutine(HideDialogueBox());
    }
    
    private void Awake()
    {
        Instance = this;
    }

    // Dialogue Dialogue; 
    public void Update()
    {
        if (!_isInputBlocked){
            if (Input.GetKeyDown(KeyCode.F))
            { 
                if (_isTyping)
                {
                    // e presses before is typing so need workaround
                    _skip -= 1;
                }
                else
                {
                    ++_current_line;
                    // check if have next line, end speech or play next 
                    AudioStatic.PlaySFX(TEXT, 0.2f); //sound effect click
                    
                    if (_current_line < _currentDialogueData.Lines.Count)
                    {   
                        _skip = 1;
                        showDialogueCoroutine = StartCoroutine(ShowDialogue(_currentDialogueData.Lines[_current_line]));
                    }
                    else
                    {   
                        EndDialogue();
                    } 
                }
            }
        }
    }

 
    //! called by player script to fetch dialogue and play it
    public IEnumerator HandleDialogue(DialogueData dialogueData, Vector3 position)
    {    
        if (!Game.DialogueBox.activeSelf) 
        {
            OnShowDialog?.Invoke(); //-> c gamestate
            _currentDialogueData = dialogueData;
            Game.DialogueText.text = "";
            _current_line = 0; 
            showDialogueCoroutine = StartCoroutine(ShowDialogue(_currentDialogueData.Lines[_current_line]));
            StartCoroutine(ScaleDialogueBox(true, SHOW_DURATION)); // show/hide box  
            // if error, check if have text in npc 

            _position = position;
            yield return null; // Wait until the hiding dialogue box is complete
        }
    }

    //! scroll text
    public IEnumerator ShowDialogue(string line)
    {
        _isTyping = true;
        Game.DialogueText.text = "";
        
         AudioStatic.StopSFX(_audioSource);
        _audioSource = AudioStatic.PlaySFX(TEXT, 0.2f, true);
        foreach (var letter in line.ToCharArray())        
        {
            // _skip text scroll
            if (_skip == 0)
            {
                _skip = 1;
                Game.DialogueText.text = line;
                AudioStatic.StopSFX(_audioSource);
                break;
            }
            Game.DialogueText.text += letter;
            yield return new WaitForSeconds(1f / TEXT_SPEED);
        }
        _isTyping = false;
        AudioStatic.StopSFX(_audioSource);
    }

    private IEnumerator HideDialogueBox()
    { 
        OnHideDialog?.Invoke(); //-> c gamestate
        yield return StartCoroutine(ScaleDialogueBox(false, HIDE_DURATION)); 
        Game.DialogueText.text = "";
        _skip = 1; 
    }












    private IEnumerator ScaleDialogueBox(bool show, float duration)
    {
        Vector3 targetScale = show ? Vector3.one : Vector3.zero;
        Vector3 initialScale = show ? Vector3.zero : Vector3.one;
        Game.DialogueBox.transform.localScale = initialScale;
        Game.DialogueBox.SetActive(true);

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            if (show)
            {
                if (Game.DialogueBox.transform.localScale.x > 0.5f) _isInputBlocked = false;
                t = Mathf.SmoothStep(0f, 1f, Mathf.Pow(t, EASE_SPEED)); // Apply adjustable ease-out effect
            }
            else
            {
                t = Mathf.Lerp(0f, 1f, t); // Linear interpolation for hiding
            }
 
            Game.DialogueBox.transform.localScale = Vector3.Lerp(initialScale, targetScale, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        Game.DialogueBox.transform.localScale = targetScale;

        if (!show)
        { 
            StopCoroutine(showDialogueCoroutine);
            Game.DialogueBox.SetActive(false);
            AudioStatic.StopSFX(_audioSource);
            _isInputBlocked = true;
        } 
    }
 
}


 