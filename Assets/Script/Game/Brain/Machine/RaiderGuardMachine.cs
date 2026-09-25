public class RaiderGuardMachine : RaiderMachine
{
    public static new Info CreateInfo()
    {
        return new EnemyInfo()
        {
            HealthMax = 16,
            DistAlert = 10,
            DistDisengage = 20,
            DistRoam = 2,
        };
    }
}
