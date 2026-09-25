using UnityEngine;

public class MobAttackShoot : MobState
{ 

    public override void OnEnterState()
    {
        Item equipment = Info.Equipment?.Info;
        ProjectileInfo projectile = equipment?.ProjectileInfo;
        if (projectile == null)
        {
            Machine.SetState<DefaultState>();
            return;
        }

        if (Info is PlayerInfo player)
        {
            if (AmmoRegistry.IsGun(equipment.ID))
            {
                ID ammo = AmmoRegistry.PickFor(equipment.ID, player.Storage);
                if (ammo == ID.Null)
                {
                    Machine.SetState<DefaultState>();
                    return;
                }
                player.Storage.RemoveItem(ammo);
                projectile = AmmoRegistry.GetProjectile(ammo);
            }
            else if (projectile.Ammo != ID.Null)
            {
                if (player.Storage.GetAmount(projectile.Ammo) == 0)
                {
                    Machine.SetState<DefaultState>();
                    return;
                }
                player.Storage.RemoveItem(projectile.Ammo);
            }
        }

        Audio.PlaySFX(equipment.Sfx);
        Info.SpriteToolEffect.localPosition = Vector3.right * equipment.ProjectileOffset;
        Info.Animator.speed = equipment.Speed; 
        Info.Animator.Play("EquipShoot", 0, 0f);
         
        Info.SpeedModifier = 0.3f;

        Vector3 direction = Info.GetDirection();
        
        ProjectileSync.SpawnProjectile(Info,
            Info.SpriteToolTrack.position + direction * equipment.ProjectileOffset,
            Info.AimPosition,
            projectile,
            Info.targetHitboxType, equipment.ID);

        if (Main.PlayerInfo == Info)
            ScreenShake.Shake(40f, 0.035f, 1f / 60f, direction);
        
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