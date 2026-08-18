using System.Collections;
using UnityEngine;

/// <summary>Melee attack that stops all movement while telegraphing and swinging,
/// then resumes chasing. Mirrors the hound's "freeze in place to swing" behavior.
/// Shared by the bear, spider, harpy, lich, and mannequin.</summary>
public class MobAttackStopSwing : MobState
{
    private readonly ProjectileInfo _projectileInfo;
    private CoroutineTask _task;

    private const float TelegraphTime = 0.1f;
    private const float SwingTime = 0.167f;
    private const float CooldownTime = 1f;

    public MobAttackStopSwing(ProjectileInfo projectileInfo)
    {
        updateMode = global::Module.UpdateMode.Everyone;
        _projectileInfo = projectileInfo;
    }

    public override void OnEnterState()
    {
        _task?.Stop();
        _task = null;
        Info.Animator.speed = Main.PlayerInfo == Info ? 0.7f : 0.3f;
        Info.SpeedModifier = 0f; // freeze in place while winding up
        Info.Animator.Play("EquipSwingTelegraph", 0, 0f);
        if (Helper.IsHost() || Info.IsOwner())
            _task = new CoroutineTask(SwingRoutine());
    }

    private IEnumerator SwingRoutine()
    {
        yield return new WaitForSeconds(TelegraphTime / Info.Animator.speed);
        Info.Animator.speed = 1f;
        Info.Animator.Play("EquipSwing", 0, 0f);
        Attack();

        yield return new WaitForSeconds(SwingTime / Info.Animator.speed);
        Info.Animator.speed = 1f;
        Audio.PlaySFX(SfxID.HitMob);
        Info.Animator.Play("EquipSwingCooldown", 0, 0f);

        yield return new WaitForSeconds(CooldownTime / Info.Animator.speed);
        Info.Animator.speed = 1f;
        Info.Animator.Play("EquipIdle", 0, 0f);
        Machine.SetState<DefaultState>();
    }

    private void Attack()
    {
        Projectile.Spawn(Info.SpriteToolTrack.position, Info.AimPosition,
            _projectileInfo, Info.targetHitboxType, Info);
    }

    public override void OnExitState()
    {
        _task?.Stop();
        _task = null;
        Info.Animator.speed = 1f;
        Info.Animator.Play("EquipIdle", 0, 0f);
        Info.SpeedModifier = 1f;
    }
}