/// <summary>A venomous spider variant. Its bite applies a Blood Clot debuff that
/// temporarily reduces the victim's max health by 2 while it is active.</summary>
public class ViperMachine : SpiderMachine
{
    private static readonly StatusEffect BloodClot = new StatusEffect(
        ID.BloodClot, EffectType.MaxHealthPenalty, duration: 20f, tickInterval: 1f, amountPerTick: 2, name: "Blood Clot");

    protected override StatusEffect BiteHitEffect => BloodClot;

    /// <summary>Unlike its docile spider ancestor, the viper is an aggressive
    /// ambush predator: it locks onto the player on sight (like other hostile
    /// mobs) and then uses the shared spider brain to chase, strafe and strike.</summary>
    public override void OnUpdate()
    {
        UpdateAggro();
        base.OnUpdate();
    }

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
            CharSprite = ID.Viper,
        };
    }
}
