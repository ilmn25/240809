using System;

public class DestructableMachine : StructureMachine, IActionPrimaryResource
{
    public override void OnStart()
    { 
        AddModule(new SpriteOrbitModule()); 
        AddModule(new StructureSpriteCullModule());   
    }
 
}