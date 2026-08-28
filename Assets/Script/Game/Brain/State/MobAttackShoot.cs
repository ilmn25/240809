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

        ProjectileInfo projectile = Info.Equipment.Info.ProjectileInfo;

        // Player-held guns fire whatever accepted ammo the shooter carries (any
        // bullet for regular guns, shotgun rounds for the shotgun); the ammo's
        // projectile is used and one round is consumed. Ammo is resolved exactly
        // once here, so PlayerMachine.Attack no longer handles it. Throwable
        // weapons (spear) and non-player shooters (scouts, turrets) keep the
        // equipment's own projectile + ammo as-is.
        if (Info is PlayerInfo player)
        {
            if (AmmoRegistry.IsGun(Info.Equipment.ID))
            {
                ID ammo = AmmoRegistry.PickFor(Info.Equipment.ID, player.Storage);
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

        Audio.PlaySFX(Info.Equipment.Info.Sfx);
        Info.SpriteToolEffect.localPosition = Vector3.right * Info.Equipment.Info.ProjectileOffset;
        Info.Animator.speed = Info.Equipment.Info.Speed; 
        Info.Animator.Play("EquipShoot", 0, 0f);
         
        Info.SpeedModifier = 0.3f;

        Vector3 direction = Info.GetDirection();
        
        ProjectileSync.SpawnProjectile(Info,
            Info.SpriteToolTrack.position + direction * Info.Equipment.Info.ProjectileOffset,
            Info.AimPosition,
            projectile,
            Info.targetHitboxType, Info.Equipment.ID);

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