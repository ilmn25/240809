/// <summary>Stationary reload for the turret — plays the reload animation in place
/// (no strafing/evading) before resuming fire.</summary>
public class TurretReloadState : MobState
{
    public override void OnEnterState()
    {
        Info.Animator.speed = 1.2f;
        Info.Animator.Play("EquipReload", 0, 0f);
        Audio.PlaySFX(Info.Equipment.Info.Sfx);
    }

    public override void OnUpdateState()
    {
        if (Info.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
        {
            Audio.PlaySFX(Info.Equipment.Info.Sfx);
            Info.Animator.speed = 1f;
            Info.Animator.Play("EquipIdle", 0, 0f);
            Machine.SetState<DefaultState>();
        }
    }
}
