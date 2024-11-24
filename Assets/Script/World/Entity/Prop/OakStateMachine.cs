public class OakStateMachine : TreeStateMachine
{
    protected override void Initialize(ref string item, ref int health)
    {
        item = "backroom";
        health = 3;
    }
}