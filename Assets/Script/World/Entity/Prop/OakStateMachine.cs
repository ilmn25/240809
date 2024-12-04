public class OakStateMachine : TreeStateMachine
{
    protected override void Initialize(ref string item, ref int health)
    {
        item = "wood";
        health = 3;
    }
}