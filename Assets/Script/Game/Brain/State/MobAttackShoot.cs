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
        Info.SpriteToolEffect.localPosition = Vector3.right * Info.Equipment.Info.ProjectileOffset;
        Info.Animator.speed = Info.Equipment.Info.Speed; 
        Info.Animator.Play("EquipShoot", 0, 0f);
         
        Info.SpeedModifier = 0.3f;

        Vector3 direction = Info.GetDirection();
        
        Projectile.Spawn(Info.SpriteToolTrack.position + direction * Info.Equipment.Info.ProjectileOffset,
            Info.AimPosition,
            Info.Equipment.Info.ProjectileInfo,
            Info.targetHitboxType, Info);
        
        ViewPort.StartScreenShake(1, 0.035f, direction);
        
        Entity.SpawnItem(ID.Casing, Info.position + Vector3.up * 0.5f, 1, false, 
            (Vector3.up -direction) * 5, 15000);
    }
 
    public override void OnUpdateState()
    {
        if (Info.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
        {
            Info.Animator.speed = 1f;
            Info.Animator.Play("EquipIdle", 0, 0f);
            Info.SpriteToolEffect.localPosition = Vector3.zero;
            Machine.SetState<DefaultState>();
        } 
    }

    public override void OnExitState()
    {
        Info.SpeedModifier = 1f;
    }
}