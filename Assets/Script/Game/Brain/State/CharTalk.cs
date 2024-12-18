public class CharTalk : State {
    public Dialogue Dialogue; 

    public CharTalk(Dialogue dialogue)
    {
        Dialogue = dialogue;
    }
 
    public void OnEndDialogue()
    {
        Parent.SetState<NPCChase>();
    }
    
    public override void OnEnterState()
    {
        GUIDialogue.StartDialogue(this);
    }
}