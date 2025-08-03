 
using UnityEngine;

public class PlayerMachine : Machine
{
    public override void OnInitialize()
    {
        AddModule(new PlayerMovementModule());
        AddModule(new PlayerTerraform());
        AddModule(new PlayerAnimationModule()); 
        AddState(new EquipState());  
        AddState(new PlayerState());  
    }
} 
