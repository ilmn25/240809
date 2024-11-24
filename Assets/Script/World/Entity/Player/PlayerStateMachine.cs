
public class PlayerStateMachine : EntityStateMachine
{
    protected override void OnAwake()
    {
        AddState(new PlayerActive(), true);
    }
  
} 
class PlayerActive : EntityState { 
    public override void StateUpdate() {
        PlayerMovementStatic.Instance.HandleMovementUpdate();
        PlayerAnimationStatic.Instance.HandleAnimationUpdate(); 
        PlayerChunkEditStatic.Instance.HandleTerraformUpdate(); 
        PlayerInteractStatic.Instance.HandleInteractionUpdate();
        PlayerInventoryStatic.Instance.HandleInventoryUpdate();
        PlayerStatusStatic.Instance.HandleStatusUpdate();
    }
}

