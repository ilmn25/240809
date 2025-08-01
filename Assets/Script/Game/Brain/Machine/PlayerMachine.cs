 
using UnityEngine;

public class PlayerMachine : Machine
{
    public override void OnInitialize()
    {
        AddModule(new PlayerMovementModule());
        AddModule(new PlayerAnimationModule()); 
        AddModule(new ToolAnimationModule()); 
        AddState(new InventoryState());  
        AddState(new PlayerState());  
    }
} 
