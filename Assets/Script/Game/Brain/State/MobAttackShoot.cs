using UnityEngine;

public class MobAttackShoot : MobState
{ 

    public override void OnEnterState()
    {
        if (Info.Equipment.Info.ProjectileInfo == null)
        {
            Machine.SetState<DefaultState>();
            return;
        } 
        Audio.PlaySFX(Info.Equipment.Info.Sfx);
        Info.Animator.speed = Info.Equipment.Info.Speed; 
        Info.Animator.Play("EquipShoot", 0, 0f);  
    }
 
    public override void OnUpdateState()
    {
        if (Info.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
        {
            Info.Animator.speed = 1f;
            Info.Animator.Play("EquipIdle", 0, 0f);
            Machine.SetState<DefaultState>();
        } 
    }
}