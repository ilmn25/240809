public class OakMachine : EntityMachine, ILeftClick
{
    public override void OnAwake()
    {
        State = new TreeStateMachine("wood", 5);
    }

    public void OnLeftClick()
    {
        ((TreeStateMachine)State).Hit();
    }
}