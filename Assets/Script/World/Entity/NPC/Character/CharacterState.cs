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
        _esm.SetState<NPCIdle>();
    }
    
    public override void OnEnterState()
    {
        if (!Game.DialogueBox.activeSelf)
            DialogueBoxSingleton.Instance.StartCoroutine(DialogueBoxSingleton.Instance.StartDialogue(this));
        else
            _esm.SetState<NPCIdle>();
    }

    public override void StateUpdate() {}
 
    public override void OnExitState() {}
}