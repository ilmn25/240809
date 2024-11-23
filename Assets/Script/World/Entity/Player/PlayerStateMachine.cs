
public class PlayerStateMachine : EntityStateMachine
{
    void Awake()
    {
        AddState(new PlayerActive(), true);
    }

    public override void OnEnable()
    { 
    }

    protected override void LogicUpdate()
    { 
    }
} 
class PlayerActive : EntityState { 
    public override void OnEnterState() {}
    public override void StateUpdate() {
        PlayerMovementStatic.Instance.HandleMovementUpdate();
        PlayerAnimationStatic.Instance.HandleAnimationUpdate(); 
        PlayerChunkEditStatic.Instance.HandleTerraformUpdate(); 
        PlayerInteractStatic.Instance.HandleInteractionUpdate();
        PlayerInventoryStatic.Instance.HandleInventoryUpdate();
        PlayerStatusStatic.Instance.HandleStatusUpdate();
    }
    public override void OnExitState() {}
}

