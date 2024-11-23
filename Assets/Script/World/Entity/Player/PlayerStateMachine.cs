public class PlayerStateMachine : EntityStateMachine
{
    void Awake()
    {
    }

    public override void OnEnable()
    {
        throw new System.NotImplementedException();
    }

    protected override void LogicUpdate()
    {
        PlayerMovementStatic.Instance.HandleMovementUpdate();
        PlayerAnimationStatic.Instance.HandleAnimationUpdate(); 
    }
}