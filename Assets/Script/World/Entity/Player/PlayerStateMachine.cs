
using UnityEngine;

public class PlayerStateMachine : EntityStateMachine
{
    protected override void OnAwake()
    {
        AddState(new PlayerActive(), true);
        transform.position = new Vector3( 
            WorldStatic.CHUNK_SIZE * WorldGenStatic.xSize / 2,
            WorldStatic.CHUNK_SIZE * WorldGenStatic.ySize + 15,
            WorldStatic.CHUNK_SIZE * WorldGenStatic.zSize / 2);
    }

    protected override void LogicUpdate()
    {
        if (Game.Player.transform.position.y < -50)
        {
            MapCullStatic.Instance.ForceRevertMesh(); 
            transform.position = new Vector3(Game.Player.transform.position.x , WorldStatic.World.Bounds.y + 40, Game.Player.transform.position.z);
        }
        
        if (Input.GetKeyDown(KeyCode.O)) 
            Entity.SpawnPrefab("chito", transform.position + Vector3.up);
    }
} 
class PlayerActive : EntityState { 
    public override void StateUpdate() { 
        PlayerMovementSingleton.Instance.HandleMovementUpdate();
        PlayerAnimationSingleton.Instance.HandleAnimationUpdate(); 
        PlayerChunkEditSingleton.Instance.HandleTerraformUpdate(); 
        PlayerInventorySingleton.Instance.HandleInventoryUpdate();
        PlayerStatusSingleton.Instance.HandleStatusUpdate();
    }
}

