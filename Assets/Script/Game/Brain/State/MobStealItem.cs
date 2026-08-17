using UnityEngine;

/// <summary>Gnome behavior: path to the player's dropped item and grab it once
/// within reach. Exits to DefaultState on its own when the item vanishes, so the
/// gnome never gets stuck chasing an item that no longer exists.</summary>
public class MobStealItem : MobState
{
    private const float PickupRadius = 1.2f;

    public override void OnEnterState()
    {
        Module<PathingModule>().SetTarget(PathingTarget.Target);
    }

    public override void OnUpdateState()
    {
        if (Info.Target == null || Info.Target.Destroyed)
        {
            Info.CancelTarget();
            return;
        }

        if (Vector3.Distance(Machine.transform.position, Info.Target.position) <= PickupRadius)
        {
            if (Machine is GnomeMachine gnome)
                gnome.GrabAndFlee();
            return;
        }

        if (Info.PathingStatus != PathingStatus.Pending)
            Module<PathingModule>().SetTarget(PathingTarget.Target);
    }
}
