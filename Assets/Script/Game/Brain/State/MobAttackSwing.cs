using UnityEngine;

class MobAttackSwing : State {
    private MobInfo MobInfo => Machine.GetModule<MobInfo>();
    private Animator Animator => MobInfo.Animator;

    public override void OnEnterState()
    {
        Animator.speed = 1f; 
        Animator.Play("EquipSwing", 0, 0f);  
        Projectile.Spawn(Machine.transform.position,Module<MobInfo>().Target.transform.position,
            MobInfo.Equipment.ProjectileInfo, HitboxType.Friendly);
    }
    
    public override void OnUpdateState() {
        AnimatorStateInfo stateInfo = Animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.normalizedTime >= 1f)
        {
            if (stateInfo.IsName("EquipSwing"))
            {
                Audio.PlaySFX(MobInfo.Equipment.Sfx, 0.5f);
                Animator.speed = MobInfo.Equipment.Speed;
                Animator.Play("EquipSwingCooldown", 0, 0f);
            }
            else if (stateInfo.IsName("EquipSwingCooldown"))
            {
                Animator.speed = 1f;
                Animator.Play("EquipIdle", 0, 0f);
                Machine.SetState<DefaultState>();
            }
        } 
    }
}