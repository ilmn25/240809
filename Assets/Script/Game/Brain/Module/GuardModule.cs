using UnityEngine;

public class GuardModule : MobModule
{
    public Vector3 HomePosition;

    private const float DefaultLeashRadius = 20f;

    public float LeashRadius = DefaultLeashRadius;

    public bool IsBeyondLeash =>
        Vector3.Distance(Machine.transform.position, HomePosition) > LeashRadius;

    public static GuardModule Attach(Info info, Vector3 home, float leashRadius = DefaultLeashRadius) =>
        info?.Machine != null ? Attach(info.Machine, home, leashRadius) : null;

    public static GuardModule Attach(Machine machine, Vector3 home, float leashRadius = DefaultLeashRadius)
    {
        GuardModule guard = machine.GetModule<GuardModule>();
        if (guard == null) guard = machine.AddModule(new GuardModule());
        if (machine.GetState<MobReturnHome>() == null)
            machine.AddState(new MobReturnHome());
        guard.HomePosition = home;
        guard.LeashRadius = leashRadius;
        return guard;
    }

    public override void Update()
    {
        if (!Helper.IsHost()) return;

        if (IsBeyondLeash && !Machine.IsCurrentState<MobReturnHome>())
        {
            PathingModule pathing = Machine.GetModule<PathingModule>();
            if (pathing != null) pathing.HomePosition = HomePosition;

            Info.CancelTarget();
            Machine.SetState<MobReturnHome>();
        }
    }
}
