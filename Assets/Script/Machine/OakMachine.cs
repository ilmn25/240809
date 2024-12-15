public class OakMachine : EntityMachine, ILeftClick
{
    public override void OnAwake()
    {
        State = new TreeState("wood", 5);
    }

    public void OnLeftClick()
    {
        ((TreeState)State).Hit();
    }
}