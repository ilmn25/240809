using UnityEngine;

public class DialogueState : MobState {
    public Dialogue DialogueData;

    public override bool IsInteraction => ((EntityMachine)Machine).Info is DynamicInfo { IsNPC: true };

    public DialogueState(Dialogue dialogue)
    {
        DialogueData = dialogue;
    }

    public override void OnEnterState()
    {
        Audio.PlaySFX(SfxID.Notification);
        Info.PathingStatus = PathingStatus.Reached;
        Info.Direction = Vector3.zero;
        ShowDialogue();
    }
    
    public override void OnUpdateState()
    {
        if (!global::Dialogue.Showing ||
            Helper.SquaredDistance(Main.Player.transform.position, Machine.transform.position) > 5 * 5)
        {
            Machine.SetState<DefaultState>();
        }
    }
     

    public override void OnExitState()
    {
        if (global::Dialogue.Target == DialogueData)
            global::Dialogue.Show(false);
    }

    protected void ShowDialogue()
    {
        global::Dialogue.Target = DialogueData;
        if (global::Dialogue.Showing)
            global::Dialogue.Refresh();
        else
            global::Dialogue.Show(true);
    }
}

public class MessageState : DialogueState {
    public MessageState(Dialogue dialogue) : base(dialogue) { }

    public override void OnEnterState()
    {
        if (Main.GUIDialogue.activeSelf)
            return;
        ShowDialogue();
    }
}