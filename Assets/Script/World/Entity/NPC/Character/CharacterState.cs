using UnityEngine;

public class CharTalk : EntityState {
    public EntityStateMachine _esm;
    public DialogueData _dialogueData; 

    public CharTalk(EntityStateMachine esm, DialogueData dialogueData)
    {
        _esm = esm;
        _dialogueData = dialogueData;
    }
 
    public void OnEndDialogue()
    {
        _esm.SetState<NPCChase>();
    }
    
    public override void OnEnterState()
    {
        GUIDialogueSingleton.Instance.StartDialogue(this);
    }
 
}