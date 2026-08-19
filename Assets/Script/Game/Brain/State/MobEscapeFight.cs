using UnityEngine;

/// <summary>Runs away like MobEscape, but turns and attacks the target if it gets
/// too close. After the attack finishes the machine's OnUpdate resumes escaping.</summary>
class MobEscapeFight<T> : MobEscape where T : MobState
{
    public override void OnUpdateState()
    {
        if (Info.Target != null &&
            Vector3.Distance(Machine.transform.position, Info.Target.position) < Info.DistAttack)
        {
            Info.AimPosition = Info.Target.position;
            Machine.SetState<T>();
            return;
        }
        TryEndEscape();
    }
}
