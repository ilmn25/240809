public class EquipSelectState : MobState
{
    protected virtual string ClipName => "EquipSelect";

    public override void OnEnterState()
    {
        Info.Animator.speed = 1f;
        Info.Animator.Play(ClipName, 0, 0f);
    }
    
    public override void OnUpdateState()
    {  
        if (Info.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
        {
            Machine.SetState<DefaultState>();
            Info.Animator.Play("EquipIdle", 0, 0f); 
        } 
    }
}

public class MobCraftState : EquipSelectState
{
    protected override string ClipName => "EquipCraft";
}