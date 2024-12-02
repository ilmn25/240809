
using UnityEngine;

public class PlayerStateMachine : EntityStateMachine
{
    protected override void OnAwake()
    {
        AddState(new PlayerActive(), true);
        transform.position = new Vector3( 
            WorldSingleton.CHUNK_SIZE * WorldGenSingleton.xSize / 2,
            WorldSingleton.CHUNK_SIZE * WorldGenSingleton.ySize + 15,
            WorldSingleton.CHUNK_SIZE * WorldGenSingleton.zSize / 2);
    }

    protected override void LogicUpdate()
    {
        if (Game.Player.transform.position.y < -50)
        {
            MapCullSingleton.Instance.ForceRevertMesh(); 
            transform.position = new Vector3(Game.Player.transform.position.x , WorldSingleton.World.Bounds.y + 40, Game.Player.transform.position.z);
        }
        
        if (Input.GetKeyDown(KeyCode.O)) 
            Entity.SpawnPrefab("chito", transform.position + Vector3.up);
    }
} 
class PlayerActive : EntityState { 
    public override void StateUpdate() { 
        PlayerMovementSingleton.Instance.HandleMovementUpdate();
        PlayerAnimationSingleton.Instance.HandleAnimationUpdate(); 
        PlayerChunkEditSingleton.Instance.HandleChunkEditInput(); 
        PlayerInventorySingleton.Instance.HandleInventoryUpdate();
        PlayerStatusSingleton.Instance.HandleStatusUpdate();
    }
}

