using UnityEngine;

/// <summary>Bear melee attack: stops all movement while telegraphing and swinging,
/// then resumes chasing. Mirrors the hound's "freeze in place to swing" behavior.</summary>
public class MobAttackBearSwing : MobState
{
    public MobAttackBearSwing() { updateMode = global::Module.UpdateMode.Everyone; }

    private readonly ProjectileInfo _projectileInfo = new ContactDamageProjectileInfo {
        Damage = 3,
        Knockback = 14,
        CritChance = 0.1f,
        Radius = 0.9f,
    };

    public override void OnEnterState()
    {
        Info.Animator.speed = Main.PlayerInfo == Info ? 0.7f : 0.3f;
        Info.SpeedModifier = 0f; // hound-style: freeze in place while winding up
        Info.Animator.Play("EquipSwingTelegraph", 0, 0f);
    }

    public override void OnUpdateState()
    {
        if (!Helper.IsHost() && !Info.IsOwner()) return;

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
                Info.Animator.speed = 1;
                Audio.PlaySFX(SfxID.HitMob);
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

    private void Attack()
    {
        Projectile.Spawn(Info.SpriteToolTrack.position, Info.AimPosition,
            _projectileInfo, Info.targetHitboxType, Info);
    }

    public override void OnExitState()
    {
        Info.Animator.speed = 1f;
        Info.Animator.Play("EquipIdle", 0, 0f);
        Info.SpeedModifier = 1f;
    }
}