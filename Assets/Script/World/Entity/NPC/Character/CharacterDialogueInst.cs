using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CharacterDialogueInst : MonoBehaviour
{
    [SerializeField] 
    public Dialogue _Dialogue; //! the dialogue input field
    
    private Animator _animator;  
    //! make it easier to call animator component later
    void Awake()
    {
        _animator = transform.Find("sprite").GetComponent<Animator>(); 
    }

    //! this npc instance signalled by player script, send instance dialogue to c dialogue 
    public void interact() //  <- player interaction script
    { 
        StartCoroutine(GUIDialogueStatic.Instance.TransmitDialogue(_Dialogue, transform.position)); //-> c dialogue
        //! timer to play pinch and then go back to idle
        Invoke("invoke", 1);
        _animator.Play("pinch");
    }

    //! timer to play pinch and then go back to idle

    private void invoke()
    {
        _animator.Play("idle");
    }

}
