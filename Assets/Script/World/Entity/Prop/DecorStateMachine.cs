public class DecorStateMachine : EntityStateMachine
{ 
    protected override void OnAwake()
    {
        AddState(new Idle(), true);
    }
 
    private void OnMouseDown()
    {
        WipeEntity();
    } 
}