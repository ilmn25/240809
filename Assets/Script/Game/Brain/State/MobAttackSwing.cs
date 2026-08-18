using System.Collections;
using UnityEngine;


class MobAttackSwing : MobState {

    public MobAttackSwing() { updateMode = global::Module.UpdateMode.Everyone; }

    private Item _equipment;
    private CoroutineTask _task;

    private const float TelegraphTime = 0.1f;
    private const float SwingTime = 0.167f;
    private const float CooldownTime = 1f;

    public void Attack()
    {
        if (_equipment?.ProjectileInfo != null && Info.Equipment != null)
        {
            Vector3 direction = Info.GetDirection();
            ProjectileSync.SpawnProjectile(Info,
                Info.SpriteToolTrack.position + direction * _equipment.ProjectileOffset,
                Info.AimPosition,
                _equipment.ProjectileInfo,
                Info.targetHitboxType, Info.Equipment.ID);
        } 
    }
    
    public override void OnEnterState()
    { 
        _task?.Stop();
        _task = null;
        Info.Animator.speed = Main.PlayerInfo == Info ? 0.7f : 0.3f;
        Info.SpriteToolEffect.localPosition = new Vector3(0.8f, -0.3f, 0); 
        _equipment = Info.Equipment.Info; 
        Info.SpeedModifier = 0.25f;
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
        Info.Animator.speed = Main.CreativeMode ? 70 : _equipment.Speed;
        Audio.PlaySFX(_equipment.Sfx);
        Info.SpeedModifier = 0.8f;
        Info.Animator.Play("EquipSwingCooldown", 0, 0f);

        yield return new WaitForSeconds(CooldownTime / Info.Animator.speed);
        Info.Animator.speed = 1f;
        Info.Animator.Play("EquipIdle", 0, 0f);
        Machine.SetState<DefaultState>();
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