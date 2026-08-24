/// <summary>A venomous spider variant. Its bite applies a Blood Clot debuff that
/// temporarily reduces the victim's max health by 2 while it is active.</summary>
public class ViperMachine : SpiderMachine
{
    private static readonly StatusEffect BloodClot = new StatusEffect(
        ID.BloodClot, EffectType.MaxHealthPenalty, duration: 20f, tickInterval: 1f, amountPerTick: 2, name: "Blood Clot");

    protected override ProjectileInfo BiteProjectile { get; } = new ContactDamageProjectileInfo {
        Damage = 2,
        Knockback = 8,
        CritChance = 0.1f,
        Radius = 0.7f,
        HitEffect = BloodClot,
    };

    public static new Info CreateInfo()
    {
        return new EnemyInfo()
        {
            HealthMax = 18,
            DistAttack = 2,
            DistAlert = 14,
            DistDisengage = 18,
            DistRoam = 5,
            DistStrafe = 3,
            SpeedGround = 5.5f,
            SpeedAir = 7,
            PathJump = 2,
            PathAir = 4,
            CharSprite = ID.Spider,
        };
    }
}
