public class CharTalk : State {
    public Dialogue Dialogue; 

    public CharTalk(Dialogue dialogue)
    {
        Dialogue = dialogue;
    }
 
    public void OnEndDialogue()
    {
        Machine.SetState<MobChase>();
    }
    
    public override void OnEnterState()
    {
        GUIDialogue.StartDialogue(this);
    }
}