using UnityEngine;

public class DecorMachine : StructureMachine
{  
    public static Info CreateInfo()
    {
        return new Info();
    }
    public override void OnStart()
    {
        base.OnStart();
        AddState(new StaticIdle(),true);  
    }  
}