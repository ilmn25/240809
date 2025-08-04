 
using UnityEngine;

public class PlayerMachine : Machine, IHitBox
{
    public override void OnInitialize()
    { 
        PlayerData.Load(); 
        AddModule(new PlayerMovementModule()); 
        AddModule(new PlayerAnimationModule()); 
        AddModule(new PlayerTerraformModule());
        AddModule(new PlayerStatusModule(HitboxType.Friendly));
        AddState(new EquipState());  
        AddState(new PlayerState());
    }

} 
