using UnityEngine;

/// <summary>A raider variant that guards its outpost (dirty tent). Guards aggro
/// like a normal raider (near the guard itself), but deaggro and return home
/// once dragged too far from the tent via the shared <see cref="GuardModule"/>.</summary>
public class RaiderGuardMachine : RaiderMachine
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
            HealthMax = 16,
            DistAlert = 10,   // notices intruders a bit sooner than a raider
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
}
