using UnityEngine;

/// <summary>A raider variant that guards its outpost (dirty tent). It watches
/// (faces) anyone who approaches the tent, and only chases/attacks once an
/// intruder gets close to the guard itself. Uses the shared <see cref="GuardModule"/>
/// for tent-alert detection and leash/return-home logic.</summary>
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

    /// <summary>Guards don't aggro on sight — they only engage intruders who get
    /// close to the camp (handled by GuardModule).</summary>
    protected override void UpdateAggro() { }
}
