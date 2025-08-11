using UnityEngine;

public class DecorMachine : StructureMachine
{  
    public override void OnStart()
    {
        AddModule(new StructureSpriteCullModule()); 
        AddModule(new SpriteOrbitModule()); 
        AddState(new StaticIdle(),true);  
    }  
}