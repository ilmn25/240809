using UnityEngine;

/// <summary>A raider variant that guards its outpost (dirty tent) instead of
/// roaming the world. Uses the shared <see cref="GuardModule"/> for territorial
/// behavior (patrol near home, aggro on tent intruders, leash + return home).</summary>
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
            DistAttack = 2,
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
