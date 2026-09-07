using UnityEngine;

/// <summary>DST-style leash for mobs a spawner structure produces (tents, nests,
/// boss chest...): once dragged beyond LeashRadius it deaggros and returns home.
/// Aggro itself is handled by the base machine.</summary>
public class GuardModule : MobModule
{
    /// <summary>World position of the home (the structure that spawned this mob).</summary>
    public Vector3 HomePosition;

    private const float LeashRadius = 20f;

    public bool IsBeyondLeash =>
        Vector3.Distance(Machine.transform.position, HomePosition) > LeashRadius;

    /// <summary>Anchors a spawned mob's Info to a structure (no-op if not a mob).</summary>
    public static GuardModule Attach(Info info, Vector3 home) =>
        info?.Machine != null ? Attach(info.Machine, home) : null;

    /// <summary>Anchors a mob machine to a structure: adds the leash and the
    /// return-home state if missing, then sets its home.</summary>
    public static GuardModule Attach(Machine machine, Vector3 home)
    {
        GuardModule guard = machine.GetModule<GuardModule>();
        if (guard == null) guard = machine.AddModule(new GuardModule());
        if (machine.GetState<MobReturnHome>() == null)
            machine.AddState(new MobReturnHome());
        guard.HomePosition = home;
        return guard;
    }

    public override void Update()
    {
        if (!Helper.IsHost()) return;

        // Re-entering MobReturnHome every frame clears the path and re-runs async
        // pathfinding, so the home route never completes — kick it off only once.
        if (IsBeyondLeash && !Machine.IsCurrentState<MobReturnHome>())
        {
            // MobReturnHome paths to PathingModule.HomePosition; point it at home.
            PathingModule pathing = Machine.GetModule<PathingModule>();
            if (pathing != null) pathing.HomePosition = HomePosition;

            Info.CancelTarget();
            Machine.SetState<MobReturnHome>();
        }
    }
}
