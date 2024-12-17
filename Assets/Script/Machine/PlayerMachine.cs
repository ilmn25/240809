 
using UnityEngine;

public class PlayerMachine : Machine
{
    public override void OnInitialize()
    {
        AddModule(new PlayerMovementModule());
        AddModule(new PlayerAnimationModule()); 
        AddState(new InventoryState());  
        AddState(new PlayerState()); 
        transform.position = new Vector3( 
            World.ChunkSize * WorldGenSingleton.Size.x / 2,
            World.ChunkSize * WorldGenSingleton.Size.y + 15,
            World.ChunkSize * WorldGenSingleton.Size.z / 2);
    }
} 