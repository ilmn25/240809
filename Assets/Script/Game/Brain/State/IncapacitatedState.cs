using UnityEngine;

/// <summary>Player downed state: no movement/input, holds the death anim until revived.</summary>
public class IncapacitatedState : MobState
{
    public IncapacitatedState() { updateMode = global::Module.UpdateMode.Everyone; }

    public override void OnEnterState()
    {
        Info.Animator.speed = 1f;
        Info.Animator.Play("Die", 0, 0f);
    }

    public override void OnUpdateState()
    {
        // Keep the sprite lying down — don't let the controller return it to an upright idle pose.
        AnimatorStateInfo stateInfo = Info.Animator.GetCurrentAnimatorStateInfo(0);
        if (!stateInfo.IsName("Die"))
            Info.Animator.Play("Die", 0, 0f);
    }
}