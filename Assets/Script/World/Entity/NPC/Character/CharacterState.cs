using UnityEngine;

class CharTalk : EntityState {
    EntityStateMachine _esm;
    DialogueData _dialogueData; 

    public CharTalk(EntityStateMachine esm, DialogueData dialogueData)
    {
        _esm = esm;
        _dialogueData = dialogueData;
    }
 
    public override void OnEnterState() {
        GUIDialogueStatic.Instance.PlayDialogue(_dialogueData, _esm.transform.position);  
    }

    public override void StateUpdate()
    {
        if (Vector3.Distance(Game.Player.transform.position, _esm.transform.position) > 3) { //walk away from npc
            GUIDialogueStatic.Instance.EndDialogue();
            _esm.SetState(_esm.GetState<NPCChase>());
        }
    }
    
    public override void OnExitState() {}
}