using UnityEngine;

/// <summary>A scout variant that guards its outpost (dirty tent) instead of
/// roaming the world. Uses the shared <see cref="GuardModule"/> for territorial
/// behavior, and keeps the scout's ranged pistol combat.</summary>
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
            CharSprite = ID.Chito, // no dedicated sprite — reuse the scout
        };
    }

    public override void OnStart()
    {
        base.OnStart();
        AddModule(new GuardModule());
        AddState(new MobReturnHome());
    }
}
