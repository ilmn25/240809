
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

        PlayerMovementSingleton.Instance.HandleMovementUpdate();
        PlayerAnimationSingleton.Instance.HandleAnimationUpdate(); 
        PlayerChunkEditSingleton.Instance.HandleTerraformUpdate(); 
        PlayerInteractSingleton.Instance.HandleInteractionUpdate();
        PlayerInventorySingleton.Instance.HandleInventoryUpdate();
        PlayerStatusSingleton.Instance.HandleStatusUpdate();
    }
}

