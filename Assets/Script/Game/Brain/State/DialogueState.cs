using System;
using UnityEngine;

public class DialogueState : MobState {
    public Dialogue Dialogue; 

    public DialogueState(Dialogue dialogue)
    {
        Dialogue = dialogue;
    }

    public override void OnUpdateState()
    {
        if (!GUIDialogue.Showing || Utility.SquaredDistance(Game.Player.transform.position, Machine.transform.position) > 5*5) { //walk away from npc
            Machine.SetState<DefaultState>();
        }
    }
    
    public override void OnEnterState()
    {
        Audio.PlaySFX("chat");
        Info.PathingStatus = PathingStatus.Reached;
        Info.Direction = Vector3.zero; 
        GUIDialogue.Dialogue = Dialogue;
        GUIDialogue.Show(true);
    }

    public override void OnExitState()
    {
        GUIDialogue.Show(false);
    }
}