using UnityEngine;

/// <summary>Paths the mob back to its fixed home position (its outpost/tent).
/// Exits back to DefaultState once it arrives or gets stuck.</summary>
class MobReturnHome : MobState
{
    public override void OnEnterState()
    {
        Module<PathingModule>().SetTarget(PathingTarget.Home);
    }

    public override void OnUpdateState()
    {
        if (Info.PathingStatus != PathingStatus.Pending)
            Machine.SetState<DefaultState>();
    }
}
