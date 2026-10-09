using System;
using UnityEngine;

public class DialogueState : MobState {
    public Dialogue Dialogue; 

    public override bool IsInteraction => ((EntityMachine)Machine).Info is DynamicInfo { IsNPC: true };

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

public class MessageState : DialogueState {
    public MessageState(Dialogue dialogue) : base(dialogue) { }

    public override void OnEnterState()
    {
        if (Main.GUIDialogue.activeSelf)
            return;
        Dialogue.Target = Dialogue;
        Dialogue.Show(true);
    }
}