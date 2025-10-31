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
        if (Main.GUIDialogue.activeSelf)
        {
            Info.CancelTarget();
            return;
        }
        Audio.PlaySFX(SfxID.Notification);
        Info.PathingStatus = PathingStatus.Reached;
        Info.Direction = Vector3.zero; 
        Dialogue.Target = Dialogue;
        Dialogue.Show(true);
        GUIMain.Show(false);
    }
    
    public override void OnUpdateState()
    {
        if (!Dialogue.Showing || Helper.SquaredDistance(Main.Player.transform.position, Machine.transform.position) > 5*5) { //walk away from npc
            Machine.SetState<DefaultState>();
        }
    }
     

    public override void OnExitState()
    {
        Dialogue.Show(false);
    }
}

public class MessageState : MobState {
    public Dialogue Dialogue; 

    public MessageState(Dialogue dialogue)
    {
        Dialogue = dialogue;
    }

    public override void OnEnterState()
    {
        if (Main.GUIDialogue.activeSelf)
        {
            Info.CancelTarget();
            return;
        }
        Dialogue.Target = Dialogue;
        Dialogue.Show(true);
        GUIMain.Show(false);
    }
    
    public override void OnUpdateState()
    {
        if (!Dialogue.Showing || Helper.SquaredDistance(Main.Player.transform.position, Machine.transform.position) > 5*5) { //walk away from npc
            Machine.SetState<DefaultState>();
        }
    }
     

    public override void OnExitState()
    {
        Dialogue.Show(false);
    }
}