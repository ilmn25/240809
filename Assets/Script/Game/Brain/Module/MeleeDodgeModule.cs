using UnityEngine;

/// <summary>Predicts melee attacks and dodges them by moving back. Detects when
/// the current target is winding up a melee swing (the "EquipSwingTelegraph"
/// animator state that plays before the actual swing lands) and, if the attacker
/// is close enough to hit, triggers a dodge-back via MobEvade.
///
/// This is decoupled from any specific enemy so any melee mob can opt in by
/// adding the module (e.g. the watchdog today, other enemies later).</summary>
public class MeleeDodgeModule : MobModule
{
    /// <summary>How close the attacker must be for the dodge to trigger.</summary>
    public float DodgeRange = 3f;

    /// <summary>Cooldown (frames) between dodges so the mob doesn't spam it.</summary>
    private const int DodgeCooldown = 60;

    private int _cooldown;

    public override void Update()
    {
        if (!Helper.IsHost()) return;

        if (_cooldown > 0)
        {
            _cooldown--;
            return;
        }

        // Only dodge while idle/chasing (not mid-attack or already dodging).
        if (!Machine.IsCurrentState<DefaultState>()) return;

        Info target = Info.Target;
        if (target == null || target.Destroyed) return;

        // The attacker must be close enough to actually hit us.
        if (Vector3.Distance(target.position, Machine.transform.position) > DodgeRange) return;

        // Predict the swing: the telegraph state plays right before the hit lands.
        if (IsWindingUpMelee(target))
        {
            _cooldown = DodgeCooldown;
            Machine.SetState<MobEvade>();
        }
    }

    /// <summary>True if the given entity is currently winding up a melee swing.</summary>
    private static bool IsWindingUpMelee(Info target)
    {
        if (target is not DynamicInfo dyn || dyn.Animator == null || !dyn.Animator.isActiveAndEnabled)
            return false;
        return dyn.Animator.GetCurrentAnimatorStateInfo(0).IsName("EquipSwingTelegraph");
    }
}
