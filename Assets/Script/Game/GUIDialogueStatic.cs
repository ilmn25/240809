using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class GUIDialogueStatic : MonoBehaviour
{  
    // C_gamestate
    public event Action OnShowDialog;
    public event Action OnHideDialog;
    public static GUIDialogueStatic Instance { get; private set; }  
 
    private AudioSource _audioSource;
    private GameObject _dialogueBox; 
    private GameObject _player; 
    private TextMeshProUGUI _dialogueText;
    
    private Vector3 _position;
    private Dialogue _currentDialogue;
    private int _current_line = 0;
    Coroutine showDialogueCoroutine;
    private bool _isTyping = false;
    private bool _isInputBlocked = true;
    private int _skip = 1; 
    // 0 = _skip now 1 = unlocked 2 = locked 

    [SerializeField] private AudioClip TEXT;
    [SerializeField] private float TEXT_SPEED = 75; 
    [SerializeField] private float EASE_SPEED = 0.4f; // Adjust this value to control the ease-out speed
    [SerializeField] private float SHOW_DURATION = 0.5f; // Duration for showing animation
    [SerializeField] private float HIDE_DURATION = 0.2f; // Duration for hiding animation 
     

    private void Start()
    {
        Instance = this;  
        _dialogueBox = GameObject.Find("gui").transform.Find("dialogue_box").gameObject;
        _dialogueText = _dialogueBox.transform.Find("dialogue_text").GetComponent<TextMeshProUGUI>(); 
        _player = GameObject.Find("player"); 
    }

    // Dialogue Dialogue; 
    public void Update()
    {
        if (!_isInputBlocked){
                
            if (Vector3.Distance(_player.transform.position, _position) > 3) { //walk away from npc
                StartCoroutine(HideDialogueBox());
            }

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
                    
                    if (_current_line < _currentDialogue.Lines.Count)
                    {   
                        _skip = 1;
                        showDialogueCoroutine = StartCoroutine(ShowDialogue(_currentDialogue.Lines[_current_line]));
                    }
                    else
                    {   
                        StartCoroutine(HideDialogueBox());
                    } 
                }
            }
        }
    }

    //! called by player script to fetch dialogue and play it
    public IEnumerator TransmitDialogue(Dialogue dialogue, Vector3 position)
    {    
        if (!_dialogueBox.activeSelf) 
        {
            OnShowDialog?.Invoke(); //-> c gamestate
            _currentDialogue = dialogue;
            _dialogueText.text = "";
            _current_line = 0; 
            showDialogueCoroutine = StartCoroutine(ShowDialogue(_currentDialogue.Lines[_current_line]));
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
        _dialogueText.text = "";
        
         AudioStatic.StopSFX(_audioSource);
        _audioSource = AudioStatic.PlaySFX(TEXT, 0.2f, true);
        foreach (var letter in line.ToCharArray())        
        {
            // _skip text scroll
            if (_skip == 0)
            {
                _skip = 1;
                _dialogueText.text = line;
                AudioStatic.StopSFX(_audioSource);
                break;
            }
            _dialogueText.text += letter;
            yield return new WaitForSeconds(1f / TEXT_SPEED);
        }
        _isTyping = false;
        AudioStatic.StopSFX(_audioSource);
    }

    private IEnumerator HideDialogueBox()
    { 
        OnHideDialog?.Invoke(); //-> c gamestate
        yield return StartCoroutine(ScaleDialogueBox(false, HIDE_DURATION)); 
        _dialogueText.text = "";
        _skip = 1; 
    }












    private IEnumerator ScaleDialogueBox(bool show, float duration)
    {
        Vector3 targetScale = show ? Vector3.one : Vector3.zero;
        Vector3 initialScale = show ? Vector3.zero : Vector3.one;
        _dialogueBox.transform.localScale = initialScale;
        _dialogueBox.SetActive(true);

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            if (show)
            {
                if (_dialogueBox.transform.localScale.x > 0.5f) _isInputBlocked = false;
                t = Mathf.SmoothStep(0f, 1f, Mathf.Pow(t, EASE_SPEED)); // Apply adjustable ease-out effect
            }
            else
            {
                t = Mathf.Lerp(0f, 1f, t); // Linear interpolation for hiding
            }
 
            _dialogueBox.transform.localScale = Vector3.Lerp(initialScale, targetScale, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        _dialogueBox.transform.localScale = targetScale;

        if (!show)
        { 
            StopCoroutine(showDialogueCoroutine);
            _dialogueBox.SetActive(false);
            AudioStatic.StopSFX(_audioSource);
            _isInputBlocked = true;
        } 
    }
 
}




[System.Serializable]
public class Dialogue
{
    [SerializeField]
    public List<String> LineData;
    public List<String> Lines
    {
        get { return LineData; }
    }
}
