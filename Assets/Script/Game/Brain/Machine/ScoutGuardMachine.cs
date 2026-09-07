/// <summary>Scout variant that stands watch at a dirty tent. The tent attaches
/// a GuardModule leash, so it deaggros and returns home once dragged too far.</summary>
public class ScoutGuardMachine : ScoutMachine
{
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
}
