public abstract class ConverterMachine: StructureMachine, IActionSecondaryInteract
{ 
    public override void OnStart()
    {
        AddModule(new StructureSpriteCullModule()); 
        AddModule(new SpriteOrbitModule()); 
        AddState(new InConverterState());
    } 

    public void OnActionSecondary(Info info)
    {
        if (IsCurrentState<DefaultState>())
            SetState<InConverterState>();
        else 
            SetState<DefaultState>();
    }
}