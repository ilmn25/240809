using UnityEngine;

public class CharTalk : State {
    public DialogueData _dialogueData; 

    public CharTalk(DialogueData dialogueData)
    {
        _dialogueData = dialogueData;
    }
 
    public void OnEndDialogue()
    {
        Parent.SetState<NPCChase>();
    }
    
    public override void OnEnterState()
    {
        GUIDialogue.StartDialogue(this);
    }
 
}