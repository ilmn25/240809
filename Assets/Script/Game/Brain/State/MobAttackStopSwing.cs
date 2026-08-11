using UnityEngine;

/// <summary>Melee attack that stops all movement while telegraphing and swinging,
/// then resumes chasing. Mirrors the hound's "freeze in place to swing" behavior.
/// Shared by the bear, spider, harpy, lich, and mannequin.</summary>
public class MobAttackStopSwing : MobState
{
    private readonly ProjectileInfo _projectileInfo;

    public MobAttackStopSwing(ProjectileInfo projectileInfo)
    {
        updateMode = global::Module.UpdateMode.Everyone;
        _projectileInfo = projectileInfo;
    }

    public override void OnEnterState()
    {
        Info.Animator.speed = Main.PlayerInfo == Info ? 0.7f : 0.3f;
        Info.SpeedModifier = 0f; // freeze in place while winding up
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