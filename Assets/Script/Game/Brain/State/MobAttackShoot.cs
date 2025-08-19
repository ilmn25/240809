using UnityEngine;

public class MobAttackShoot : MobState
{ 

    public override void OnEnterState()
    {
        if (Info.Equipment.ProjectileInfo == null)
        {
            Machine.SetState<DefaultState>();
            return;
        } 
        Audio.PlaySFX(Info.Equipment.Sfx);
        Info.Animator.speed = Info.Equipment.Speed; 
        Info.Animator.Play("EquipShoot", 0, 0f);  
        
        Vector3 direction = Info.SpriteToolTrack.right;
        if (Info.SpriteToolTrack.lossyScale.x < 0f) 
            direction *= -1;
        direction.y = 0;
        direction.Normalize();
        
        // + 0.3f * Vector3.up
        Projectile.Spawn(Info.SpriteToolTrack.position + direction * Info.Equipment.ProjectileOffset,
            Info.AimPosition,
            Info.Equipment.ProjectileInfo,
            Info.TargetHitboxType, Info);
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