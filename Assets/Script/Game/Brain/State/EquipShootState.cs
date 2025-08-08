using UnityEngine;

public class EquipShootState : PlayerState
{ 
    public override void OnEnterState()
    {
        Info.IsBusy = true;   
        Info.Animator.speed = Info.GetSpeed(); 
        Audio.PlaySFX(Inventory.CurrentItemData.Sfx, 0.5f);
        Info.Animator.Play("EquipShoot", 0, 0f);  
    }
    
    public override void OnUpdateState()
    { 
        if (!Info.IsBusy) return;

        if (Info.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
        {
            Info.Animator.speed = 1f;
            Info.Animator.Play("EquipIdle", 0, 0f);
            Info.IsBusy = false; 
            Machine.SetState<DefaultState>();
        } 
    }
}