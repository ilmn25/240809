using UnityEngine;

public class EquipSelectState : State
{
    private PlayerStatusModule _playerStatusModule;
    private float _cooldownSpeed;
    private string _sfx;
    private Animator _animator; 
    public override void OnInitialize()
    {
        _playerStatusModule = Machine.GetModule<PlayerStatusModule>();
        _animator = Machine.transform.Find("sprite").GetComponent<Animator>();
    }

    public override void OnEnterState()
    {
        _playerStatusModule.IsBusy = true;   
        _animator.speed = 1f;
        _animator.Play("EquipSelect", 0, 0f); // Play from beginning
    }
    
    public override void OnUpdateState()
    { 
        if (!_playerStatusModule.IsBusy) return;

        if (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
        {
            _playerStatusModule.IsBusy = false; 
            Parent.SetState<StateEmpty>();
            _animator.Play("EquipIdle", 0, 0f); 
        } 
    }
}