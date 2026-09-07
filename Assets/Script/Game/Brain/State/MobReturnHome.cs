using UnityEngine;

/// <summary>Paths the mob back to its home. Exits back to DefaultState once it
/// arrives or gets stuck.</summary>
class MobReturnHome : MobState
{
    public override void OnEnterState()
    {
        Module<PathingModule>().SetTarget(PathingTarget.Home);
    }

    public override void OnUpdateState()
    {
        // The home is the structure's own solid cell, which a ground mob can
        // never step onto — pathing can hang on Pending forever there. Being
        // back inside the leash radius is close enough to call it home.
        GuardModule guard = Machine.GetModule<GuardModule>();
        if (Info.PathingStatus != PathingStatus.Pending ||
            (guard != null && !guard.IsBeyondLeash))
            Machine.SetState<DefaultState>();
    }
}
