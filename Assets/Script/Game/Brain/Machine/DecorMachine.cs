using UnityEngine;

public class DecorMachine : StructureMachine
{  
    public override void OnStart()
    {
        AddModule(new SpriteCullModule(SpriteRenderer)); 
        AddModule(new SpriteOrbitModule(SpriteRenderer)); 
        AddState(new StaticIdle(),true);  
    }  
}