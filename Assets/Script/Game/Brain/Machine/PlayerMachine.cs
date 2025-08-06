 
using UnityEngine;

public class PlayerMachine : Machine, IHitBox
{
    public override void OnInitialize()
    { 
        PlayerData.Load(); 
        AddModule(new PlayerStatusModule());
        AddModule(new PlayerMovementModule()); 
        AddModule(new PlayerAnimationModule()); 
        AddModule(new PlayerTerraformModule()); 
        AddState(new PlayerState());  
    }

} 
