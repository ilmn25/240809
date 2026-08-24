using UnityEngine;

/// <summary>A scout variant that guards its outpost (dirty tent) instead of
/// roaming the world. Guards aggro like a normal scout (near the guard itself),
/// but deaggro and return home once dragged too far from the tent via the shared
/// <see cref="GuardModule"/>.</summary>
public class ScoutGuardMachine : ScoutMachine
{
    /// <summary>World position of the outpost this guard protects.</summary>
    public Vector3 HomePosition
    {
        get => GetModule<GuardModule>().HomePosition;
        set => GetModule<GuardModule>().HomePosition = value;
    }

    public static new Info CreateInfo()
    {
        return new EnemyInfo()
        {
            HealthMax = 12,
            SpeedGround = 4,
            DistAttack = 18,
            DistAlert = 10,   // notices intruders a bit sooner than a scout
            DistDisengage = 20,
            DistRoam = 6,
        };
    }

    public override void OnStart()
    {
        base.OnStart();
        AddModule(new GuardModule());
        AddState(new MobReturnHome());
    }

    /// <summary>Don't re-acquire a target while dragged off the leash or already
    /// heading home — that fights the return-home pathing and freezes the guard.</summary>
    protected override void UpdateAggro()
    {
        GuardModule guard = GetModule<GuardModule>();
        if (guard.IsBeyondLeash || IsCurrentState<MobReturnHome>()) return;
        base.UpdateAggro();
    }
}
