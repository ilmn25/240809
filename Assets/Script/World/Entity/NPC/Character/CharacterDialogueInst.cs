 
using UnityEngine;
using UnityEngine.Serialization; 

public class CharacterDialogueInst : MonoBehaviour
{
    [FormerlySerializedAs("_Dialogue")] [SerializeField] 
    public DialogueData dialogueData; //! the dialogue input field
    
    private Animator _animator;  
    //! make it easier to call animator component later
    void Awake()
    {
        _animator = transform.Find("sprite").GetComponent<Animator>(); 
    }

    //! this npc instance signalled by player script, send instance dialogue to c dialogue 
    public void interact() //  <- player interaction script
    { 
         GUIDialogueStatic.Instance.PlayDialogue(dialogueData, transform.position); //-> c dialogue
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
