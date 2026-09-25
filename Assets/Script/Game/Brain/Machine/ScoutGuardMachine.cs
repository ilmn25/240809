public class ScoutGuardMachine : ScoutMachine
{
    public static new Info CreateInfo()
    {
        return new EnemyInfo()
        {
            HealthMax = 12,
            SpeedGround = 4,
            DistAttack = 18,
            DistAlert = 10,
            DistDisengage = 20,
            DistRoam = 2,
        };
    }
}
