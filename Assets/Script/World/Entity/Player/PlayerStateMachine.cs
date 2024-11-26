
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
        if (Input.GetKey(KeyCode.O)) {Entity.SpawnPrefab("megumin", StateMachine.transform.position + Vector3.up);}
        PlayerMovementStatic.Instance.HandleMovementUpdate();
        PlayerAnimationStatic.Instance.HandleAnimationUpdate(); 
        PlayerChunkEditStatic.Instance.HandleTerraformUpdate(); 
        PlayerInteractStatic.Instance.HandleInteractionUpdate();
        PlayerInventoryStatic.Instance.HandleInventoryUpdate();
        PlayerStatusStatic.Instance.HandleStatusUpdate();
    }
}

