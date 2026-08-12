using UnityEngine;

/// <summary>Lich enemy: agonizingly slow but hits hard and is very tanky.
/// Relentlessly closes on the player — get away or get crushed. Only stalks
/// during the Rapture and vanishes when it ends.</summary>
public class LichMachine : HostileMeleeMachine
{
    private static readonly ProjectileInfo CrushProjectile = new ContactDamageProjectileInfo {
        Damage = 10,
        Knockback = 30,
        CritChance = 0.1f,
        Radius = 1.2f,
    };

    protected override ProjectileInfo AttackProjectile => CrushProjectile;

    public static Info CreateInfo()
    {
        return new EnemyInfo()
        {
            HealthMax = 200,
            DistAttack = 2,
            DistAlert = 30,
            DistDisengage = 30,
            DistRoam = 5,
            SpeedGround = 0.8f,
            SpeedAir = 1.0f,
            SpeedLogic = 0.8f,
            PathJump = 1,
            PathAir = 3,
        };
    }

    public override void OnUpdate()
    {
        // The Rapture ends — vanish in a puff.
        if (Save.Inst.weather != EnvironmentType.Rapture)
        {
            if (!Info.Destroyed)
            {
                Particle.Create(transform.position, Particles.HitDust, true);
                Info.Destroy();
            }
            return;
        }

        base.OnUpdate();
    }
}