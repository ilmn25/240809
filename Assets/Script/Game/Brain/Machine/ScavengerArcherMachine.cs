/// <summary>A hungry scavenger picking at range with a shortbow — a raider's
/// poorer cousin: less health, shorter reach and a bow that hits softer than a
/// guard's longbow.</summary>
public class ScavengerArcherMachine : ScoutMachine
{
    protected override ID RangedWeapon => ID.ShortBow;

    public static new Info CreateInfo()
    {
        return new EnemyInfo
        {
            HealthMax = 6,
            SpeedGround = 4,
            DistAttack = 14,
            DistAlert = 9,
            DistDisengage = 16,
            DistRoam = 6,
            CharSprite = ID.Raider,
        };
    }
}
