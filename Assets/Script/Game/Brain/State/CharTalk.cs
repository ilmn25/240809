using UnityEngine;

public class CharTalk : MobState {
    public Dialogue Dialogue; 

    public CharTalk(Dialogue dialogue)
    {
        Dialogue = dialogue;
    }
 
    public void OnEndDialogue()
    {
        Machine.SetState<DefaultState>();
    }
    
    public override void OnEnterState()
    {
        Info.PathingStatus = PathingStatus.Reached;
        Info.Direction = Vector3.zero;
        GUIDialogue.StartDialogue(this);
    }
}