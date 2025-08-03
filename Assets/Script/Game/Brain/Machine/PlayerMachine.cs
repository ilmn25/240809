 
using UnityEngine;

public class PlayerMachine : Machine
{
    public override void OnInitialize()
    {
        AddModule(new PlayerMovementModule()); 
        AddModule(new PlayerAnimationModule()); 
        AddModule(new PlayerTerraformModule());
        AddModule(new PlayerStatusModule());
        AddState(new EquipState());  
        AddState(new PlayerState());  
    }

    public override void OnUpdate()
    { 
        GetModule<PlayerTerraformModule>().Update();
        GetModule<PlayerStatusModule>().Update();
    }
} 
