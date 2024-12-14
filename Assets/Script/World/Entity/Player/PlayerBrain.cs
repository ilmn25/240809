 
using UnityEngine;

public class PlayerBrain : StateMachine
{
    public override void OnAwake()
    {
        State = new PlayerState();
        transform.position = new Vector3( 
            WorldSingleton.CHUNK_SIZE * WorldGenSingleton.xSize / 2,
            WorldSingleton.CHUNK_SIZE * WorldGenSingleton.ySize + 15,
            WorldSingleton.CHUNK_SIZE * WorldGenSingleton.zSize / 2);
    }
} 