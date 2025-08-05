using UnityEngine;

public class EquipSwingState : State
{
    private PlayerStatusModule _playerStatusModule;
    private float _cooldownSpeed;
    private Animator _animator;
    
    public override void OnInitialize()
    {
        _playerStatusModule = Machine.GetModule<PlayerStatusModule>();
        _animator = Machine.transform.Find("sprite").GetComponent<Animator>();
    }

    public override void OnEnterState()
    {
        _playerStatusModule.IsBusy = true;  
        _cooldownSpeed = PlayerStatusModule.GetSpeed();

        _animator.speed = 1f; // Reset speed for initial swing
        _animator.Play("EquipSwing", 0, 0f); // Play from beginning
    }
    
    public override void OnUpdateState()
    { 
        if (!_playerStatusModule.IsBusy) return;

        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.normalizedTime >= 1f)
        {
            if (stateInfo.IsName("EquipSwing"))
            {
                Audio.PlaySFX(Inventory.CurrentItemData.Sfx, 0.5f);
                _animator.speed = _cooldownSpeed;
                _animator.Play("EquipSwingCooldown", 0, 0f);
            }
            else if (stateInfo.IsName("EquipSwingCooldown"))
            {
                _animator.speed = 1f;
                _animator.Play("EquipIdle", 0, 0f);
                _playerStatusModule.IsBusy = false; 
                Parent.SetState<StateEmpty>();
            }
        } 
    }
}