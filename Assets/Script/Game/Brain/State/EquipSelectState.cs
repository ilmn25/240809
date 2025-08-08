using UnityEngine;

public class EquipSelectState : PlayerState
{
    public override void OnEnterState()
    {
        Info.IsBusy = true;   
        Info.Animator.speed = 1f;
        Info.Animator.Play("EquipSelect", 0, 0f); // Play from beginning
    }
    
    public override void OnUpdateState()
    { 
        if (!Info.IsBusy) return;

        if (Info.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
        {
            Info.IsBusy = false; 
            Machine.SetState<DefaultState>();
            Info.Animator.Play("EquipIdle", 0, 0f); 
        } 
    }
}