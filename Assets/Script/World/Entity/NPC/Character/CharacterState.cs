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
        _esm.SetState(_esm.GetState<NPCChase>());
    }
    
    public override void OnEnterState() {
        if (!Game.DialogueBox.activeSelf) 
            GUIDialogueStatic.Instance.PlayDialogue(this);  
        else 
            _esm.SetState(_esm.GetState<NPCChase>());
    }

    public override void StateUpdate() {}
 
    public override void OnExitState() {}
}