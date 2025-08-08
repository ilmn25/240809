using UnityEngine;

public class EquipSwingState : PlayerState
{ 
    private float _cooldownSpeed;
    public override void OnEnterState()
    {
        Info.IsBusy = true;  
        _cooldownSpeed = Info.GetSpeed();
        Audio.PlaySFX(Inventory.CurrentItemData.Sfx, 0.5f);
        Info.Animator.speed = 1f; // Reset speed for initial swing
        Info.Animator.Play("EquipSwing", 0, 0f); // Play from beginning
    }
    
    public override void OnUpdateState()
    { 
        if (!Module<PlayerInfo>().IsBusy) return;

        AnimatorStateInfo stateInfo = Info.Animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.normalizedTime >= 1f)
        {
            if (stateInfo.IsName("EquipSwing"))
            { 
                Info.Animator.speed = _cooldownSpeed;
                Info.Animator.Play("EquipSwingCooldown", 0, 0f);
            }
            else if (stateInfo.IsName("EquipSwingCooldown"))
            {
                Info.Animator.speed = 1f;
                Info.Animator.Play("EquipIdle", 0, 0f);
                Module<PlayerInfo>().IsBusy = false; 
                Machine.SetState<DefaultState>();
            }
        } 
    }
}