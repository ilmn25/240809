using UnityEngine;

class MobAttackSwing : MobState {
    
    private Item _equipment;
    public override void OnEnterState()
    { 
        Info.Animator.speed = 1f;  
        Info.Animator.Play("EquipSwing", 0, 0f);
        _equipment = Info.Equipment.Info;
        Info.SpriteToolEffect.localPosition = new Vector3(0.8f, -0.3f, 0);
        Audio.PlaySFX(_equipment.Sfx);
        Info.SpeedModifier = 0.3f;
    }
    
    public override void OnUpdateState()
    { 
        AnimatorStateInfo stateInfo = Info.Animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.normalizedTime >= 1f)
        {
            if (stateInfo.IsName("EquipSwing"))
            {  
                Info.Animator.speed = Game.BuildMode? 70 : _equipment.Speed;
                Info.Animator.Play("EquipSwingCooldown", 0, 0f);
            }
            else if (stateInfo.IsName("EquipSwingCooldown"))
            {
                Info.Animator.speed = 1f;
                Info.Animator.Play("EquipIdle", 0, 0f);
                Machine.SetState<DefaultState>(); 
            }
        } 
    }

    public override void OnExitState()
    {
        Info.Animator.speed = 1f;
        Info.Animator.Play("EquipIdle", 0, 0f); 
        Info.SpeedModifier = 1f;
    }
}