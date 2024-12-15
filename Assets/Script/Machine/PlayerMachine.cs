 
using UnityEngine;

public class PlayerMachine : Machine
{
    public override void OnInitialize()
    {
        State = new PlayerState();
        transform.position = new Vector3( 
            WorldSingleton.CHUNK_SIZE * WorldGenSingleton.xSize / 2,
            WorldSingleton.CHUNK_SIZE * WorldGenSingleton.ySize + 15,
            WorldSingleton.CHUNK_SIZE * WorldGenSingleton.zSize / 2);
    }
} 