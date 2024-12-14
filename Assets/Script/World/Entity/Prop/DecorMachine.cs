public class DecorMachine : EntityMachine
{ 
    public override void OnAwake()
    {
        State = new Idle();
    }
    //
    // private void OnMouseDown()
    // {
    //     WipeEntity();
    // } 
}