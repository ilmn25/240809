using UnityEngine;

public class EquipSelectState : MobState
{
    public override void OnEnterState()
    {
        Info.Animator.speed = 1f;
        Info.Animator.Play("EquipSelect", 0, 0f); // Play from beginning
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