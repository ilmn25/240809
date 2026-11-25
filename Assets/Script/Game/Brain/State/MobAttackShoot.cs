using UnityEngine;
using Random = UnityEngine.Random;

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

        // Ammo may be the stack in hand rather than a spare in the bag.
        if (Info is PlayerInfo player && !AmmoRegistry.TryTakeAmmo(player, Info.Equipment, out projectile))
        {
            Machine.SetState<DefaultState>();
            return;
        }

        Audio.PlaySFX(equipment.Sfx);
        Info.SpriteToolEffect.localPosition = Vector3.right * equipment.ProjectileOffset;
        Info.Animator.speed = equipment.Speed; 
        Info.Animator.Play("EquipShoot", 0, 0f);
         
        Info.SpeedModifier = 0.3f;

        Vector3 direction = Info.GetDirection();
        Vector3 origin = Info.SpriteToolTrack.position + direction * equipment.ProjectileOffset;

        // The controlled player aims with the mouse. An AI aims at the target's body and
        // leads a moving target, so a thrown spear lands where the target will be.
        Vector3 aim = Main.PlayerInfo == Info ? Info.AimPosition : Info.GetAimPoint(origin, projectile);

        ProjectileInfo weapon = equipment.ProjectileInfo;
        aim = SpreadAim(origin, aim, weapon.Spread);

        if (Helper.IsHost() || Info.IsOwner())
            Info.KnockBack(Info.position + direction * (equipment.ID == ID.Spear ? -1f : 1f), weapon.Recoil, true);

        ProjectileSync.SpawnProjectile(Info, origin, aim, projectile,
            Info.targetHitboxType, equipment.ID);

        if (Main.PlayerInfo == Info)
            ScreenShake.Shake(60f, 0.1f + weapon.Recoil * 0.02f, 0.18f, direction);

        // Only firearms eject brass: a thrown spear or a loosed arrow leaves nothing behind.
        if (AmmoRegistry.IsGun(equipment.ID))
            Entity.SpawnItem(ID.Casing, Info.position + Vector3.up * 0.5f, 1, false, 
                (Vector3.up -direction) * 5, 15000);
    }
 
    private static Vector3 SpreadAim(Vector3 origin, Vector3 aim, float spread)
    {
        if (spread <= 0f) return aim;
        Vector3 offset = aim - origin;
        return origin + Quaternion.Euler(0f, Random.Range(-spread, spread), 0f) * offset;
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