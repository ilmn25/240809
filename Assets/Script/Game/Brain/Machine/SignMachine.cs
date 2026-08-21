public class SignMachine: StructureMachine, IActionSecondaryInteract
{
    public static Info CreateInfo()
    {
        return new Info();
    }

    public override void OnStart()
    {
        base.OnStart();
        
        Dialogue dialogue = new Dialogue
        {
            Text = "illu was here\n25/08/26", 
        };
        AddState(new MessageState(dialogue)); 
    }

    public void OnActionSecondary(Info info)
    {
        if (IsCurrentState<DefaultState>())
        {
            SetState<MessageState>();
        } 
        else 
            SetState<DefaultState>();
    }
}