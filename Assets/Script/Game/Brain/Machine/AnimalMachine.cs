using UnityEngine;

/// <summary>Shared base for docile farm animals (hen, rooster, sheep, chick).
/// Handles common module setup, dialogue interaction, and gizmos. Subclasses
/// add their own states and retaliation behavior.</summary>
public abstract class AnimalMachine : GroundMobMachine, IActionSecondaryInteract
{
    public override void OnStart()
    {
        base.OnStart();

        AddState(new MobIdle());
        AddState(new MobRoam());
        AddState(new MobHit());
        AddState(new EquipSelectState());
        AddState(new DialogueState(new Dialogue { Text = DialogueText }));
    }

    /// <summary>Dialogue shown when the player interacts with this animal.</summary>
    protected abstract string DialogueText { get; }

    public void OnActionSecondary(Info info)
    {
        if (Info.Target != null) return;
        SetState<DialogueState>();
    }

    /// <summary>Point this animal at a target and start chasing it.</summary>
    public void Chase(Info target)
    {
        Info.Target = target;
        Info.ActionType = IActionType.Hit;
        Info.PathingStatus = PathingStatus.Pending;
        SetState<MobChase>();
    }

    /// <summary>Attack the current target if in range, otherwise chase it.</summary>
    protected void AttackOrChase()
    {
        if (Info.Target == null) return;
        if (Vector3.Distance(Info.Target.position, transform.position) < Info.DistAttack)
        {
            Info.AimPosition = Info.Target.position;
            SetState<MobAttackPounce>();
        }
        else
            SetState<MobChase>();
    }
}
