public class DeadState : MobState
{
    public override void OnEnterState()
    {
        Info.Animator.speed = 1f;
        Info.Animator.Play("Die", 0, 0f);
    }

    public override void OnExitState()
    {
        Info.Animator.Play("EquipIdle", 0, 0f);
    }
}