using UnityEngine;

public class BaelMachine : GroundMobMachine
{
    protected override bool UsesDoorBash => true;

    private const int AttackInterval = 150;
    private const int EnragedAttackInterval = 90;
    private const float SpecialRange = 13f;

    private static readonly ProjectileInfo Claw = new ContactDamageProjectileInfo
    {
        Damage = 8,
        Knockback = 28,
        CritChance = 0.1f,
        Radius = 1.1f,
    };

    private int _attackTimer;
    private int _pattern;
    private bool _enraged;

    public static Info CreateInfo()
    {
        return new EnemyInfo
        {
            HealthMax = 300,
            DistAttack = 2,
            DistAlert = 12,
            DistDisengage = 20,
            DistRoam = 4,
            SpeedGround = 3.2f,
            SpeedAir = 4f,
            SpeedLogic = 3f,
            PathJump = 1,
            PathAir = 3,
            Defense = 1,
            DeathSfx = SfxID.Thunder,
        };
    }

    public override void OnSetup()
    {
        base.OnSetup();
        Info.SpriteChar.localScale *= 1.5f;
    }

    public override void OnStart()
    {
        base.OnStart();

        AddState(new MobIdle());
        AddState(new MobChase());
        AddState(new MobRoam());
        AddState(new MobHit());
        AddState(new MobAttackSwing(Claw));
        AddState(new EquipSelectState());
        AddState(new BaelHellfire());
        AddState(new BaelCharge());
        AddState(new BaelNova());
        AddState(new BaelSummon());
    }

    public override void OnUpdate()
    {
        if (!_enraged && Info.Health * 2 <= Info.HealthMax)
            Enrage();

        UpdateAggro();
        if (!IsCurrentState<DefaultState>()) return;

        if (Info.Target == null)
        {
            _attackTimer = 0;
            RoamOrIdle();
            return;
        }

        if (TrySpecialAttack()) return;

        if (!EngageTarget()) RoamOrIdle();
    }

    private bool TrySpecialAttack()
    {
        if (Vector3.Distance(Info.Target.position, transform.position) > SpecialRange)
        {
            _attackTimer = 0;
            return false;
        }

        if (++_attackTimer < (_enraged ? EnragedAttackInterval : AttackInterval)) return false;

        _attackTimer = 0;
        _pattern++;

        switch (_pattern % (_enraged ? 4 : 2))
        {
            case 0: SetState<BaelHellfire>(); break;
            case 1: SetState<BaelCharge>(); break;
            case 2: SetState<BaelNova>(); break;
            default: SetState<BaelSummon>(); break;
        }
        return true;
    }

    private void Enrage()
    {
        _enraged = true;
        Info.SpeedGround *= 1.4f;
        Info.SpeedAir *= 1.4f;
        Audio.PlaySFX(SfxID.Thunder);
        Particle.Create(transform.position + Vector3.up * 0.5f, Particles.Fire, true);
        Particle.Create(transform.position, Particles.HitDust, true);
        Dialogue.ShowEvent("Bael is enraged!");
    }
}
