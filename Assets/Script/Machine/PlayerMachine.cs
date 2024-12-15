 
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
            WorldSingleton.CHUNK_SIZE * WorldGenSingleton.xSize / 2,
            WorldSingleton.CHUNK_SIZE * WorldGenSingleton.ySize + 15,
            WorldSingleton.CHUNK_SIZE * WorldGenSingleton.zSize / 2);
    }
} 