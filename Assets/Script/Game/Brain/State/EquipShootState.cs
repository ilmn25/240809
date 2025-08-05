using UnityEngine;

public class EquipShootState : State
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
        _animator.speed = PlayerStatusModule.GetSpeed(); 
        Audio.PlaySFX(Inventory.CurrentItemData.Sfx, 0.5f);
        _animator.Play("EquipShoot", 0, 0f);  
    }
    
    public override void OnUpdateState()
    { 
        if (!_playerStatusModule.IsBusy) return;

        if (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
        {
            _animator.speed = 1f;
            _animator.Play("EquipIdle", 0, 0f);
            _playerStatusModule.IsBusy = false; 
            Parent.SetState<StateEmpty>();
        } 
    }
}