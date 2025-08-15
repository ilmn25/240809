using UnityEngine;

public class DecorMachine : StructureMachine
{  
    public static Info CreateInfo()
    {
        return new Info();
    }
    public override void OnStart()
    {
        AddModule(new StructureSpriteCullModule()); 
        AddModule(new SpriteOrbitModule()); 
        AddState(new StaticIdle(),true);  
    }  
}