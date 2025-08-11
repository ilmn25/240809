using UnityEngine;

class MobAttackSwing : MobState {

    public override void OnEnterState()
    {
        if (Info.Equipment.ProjectileInfo == null)
        {
            Machine.SetState<DefaultState>();
            return;
        } 
        Info.Animator.speed = 1f; 
        Info.Animator.Play("EquipSwing", 0, 0f);   
        Projectile.Spawn(Info.SpriteToolTrack.transform.position, Info.AimPosition,
            Info.Equipment.ProjectileInfo, Info.TargetHitboxType, Machine); 
    }
    
    public override void OnUpdateState() {
        AnimatorStateInfo stateInfo = Info.Animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.normalizedTime >= 1f)
        {
            if (stateInfo.IsName("EquipSwing"))
            { 
                Audio.PlaySFX(Info.Equipment.Sfx, 0.5f);
                Info.Animator.speed = Info.Equipment.Speed;
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
}