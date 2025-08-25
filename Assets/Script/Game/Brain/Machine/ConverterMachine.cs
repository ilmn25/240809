public abstract class ConverterMachine: StructureMachine, IActionSecondaryInteract
{ 
    public override void OnStart()
    {
        base.OnStart();
        AddState(new InConverterState());
    } 

    public void OnActionSecondary(Info info)
    {
        if (IsCurrentState<DefaultState>())
            SetState<InConverterState>();
        else
        {
            SetState<DefaultState>();
        } 
    }
}