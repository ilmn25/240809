using UnityEngine;


class MobAttackSwing : MobState {
    
    private Item _equipment;

    public void Attack()
    {
        if (_equipment.ProjectileInfo != null)
        {
            Vector3 direction = Info.GetDirection();
            Projectile.Spawn(Info.SpriteToolTrack.position + direction * _equipment.ProjectileOffset,
                Info.AimPosition,
                _equipment.ProjectileInfo,
                Info.targetHitboxType, Info);
        } 
    }
    
    public override void OnEnterState()
    { 
        Info.Animator.speed = Main.PlayerInfo == Info ? 0.7f : 0.3f;
        Info.SpriteToolEffect.localPosition = new Vector3(0.8f, -0.3f, 0); 
        _equipment = Info.Equipment.Info; 
        Info.SpeedModifier = 0.25f;
        Info.Animator.Play("EquipSwingTelegraph", 0, 0f);  
    }
    
    public override void OnUpdateState()
    { 
        AnimatorStateInfo stateInfo = Info.Animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.normalizedTime >= 1f)
        {
            if (stateInfo.IsName("EquipSwingTelegraph"))
            {  
                Info.Animator.speed = 1;
                Info.Animator.Play("EquipSwing", 0, 0f);
                Attack();
            }
            else if (stateInfo.IsName("EquipSwing"))
            {  
                Info.Animator.speed = Main.BuildMode? 70 : _equipment.Speed;
                Audio.PlaySFX(_equipment.Sfx); 
                Info.SpeedModifier = 0.8f;
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