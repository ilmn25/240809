
using UnityEngine;

public class PlayerStateMachine : EntityStateMachine
{
    protected override void OnAwake()
    {
        AddState(new PlayerActive(), true);
    }
  
} 
class PlayerActive : EntityState { 
    public override void StateUpdate() {
        if (Input.GetKeyDown(KeyCode.O)) {Entity.SpawnPrefab("chito", StateMachine.transform.position + Vector3.up);}

        PlayerMovementStatic.Instance.HandleMovementUpdate();
        PlayerAnimationStatic.Instance.HandleAnimationUpdate(); 
        PlayerChunkEditStatic.Instance.HandleTerraformUpdate(); 
        PlayerInteractStatic.Instance.HandleInteractionUpdate();
        PlayerInventorySingleton.Instance.HandleInventoryUpdate();
        PlayerStatusStatic.Instance.HandleStatusUpdate();
    }
}

