using UnityEngine;

public class DecorMachine : StructureMachine
{  
    public override void OnStart()
    {
        AddState(new StaticIdle(),true); 
        AddModule(new SpriteCullModule(SpriteRenderer)); 
        AddModule(new SpriteOrbitModule(SpriteRenderer)); 
    }  
}