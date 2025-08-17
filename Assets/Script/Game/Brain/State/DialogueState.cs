using System;
using UnityEngine;

public class DialogueState : MobState {
    public Dialogue Dialogue; 

    public DialogueState(Dialogue dialogue)
    {
        Dialogue = dialogue;
    }

    public override void OnEnterState()
    {
        if (Game.GUIDialogue.activeSelf)
        {
            Machine.SetState<DefaultState>();
            Info.Target = null;
            return;
        }
        Audio.PlaySFX("chat");
        Info.PathingStatus = PathingStatus.Reached;
        Info.Direction = Vector3.zero; 
        GUIDialogue.Dialogue = Dialogue;
        GUIDialogue.Show(true);
        GUIMain.Show(false);
    }
    
    public override void OnUpdateState()
    {
        if (!GUIDialogue.Showing || Helper.SquaredDistance(Game.Player.transform.position, Machine.transform.position) > 5*5) { //walk away from npc
            Machine.SetState<DefaultState>();
        }
    }
     

    public override void OnExitState()
    {
        GUIDialogue.Show(false);
    }
}