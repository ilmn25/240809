using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>Demon Eye enemy: a flying eyeball that hovers toward the player like a
/// homing missile and bites with contact damage. Uses HoverMovementModule for movement
/// instead of ground pathing.</summary>
public class DemonEyeMachine : MobMachine
{
    private static readonly ProjectileInfo TouchProjectile = new ContactDamageProjectileInfo {
        Damage = 3,
        Knockback = 8,
        CritChance = 0.1f,
        Radius = 0.8f,
    };

    public static Info CreateInfo()
    {
        return new EnemyInfo()
        {
            HealthMax = 24,
            DistAttack = 1,
            DistAlert = 10,
            DistDisengage = 18,
            SpeedGround = 0,
            SpeedAir = 4,
            CanFly = true,
        };
    }

    public override void OnStart()
    {
        // Dive right at the player's level so it can actually reach and bite.
        AddModule(new HoverMovementModule(hoverHeight: 0.5f, stopDistance: 0.9f, turnSpeed: 2f));
        AddModule(new MobSpriteCullModule());
        AddModule(new SpriteOrbitModule());

        AddState(new MobIdle());
        AddState(new MobHit());
    }

    public override void OnUpdate()
    {
        if (!IsCurrentState<DefaultState>()) return;

        // Aggressive: lock onto the player on sight, drop once they flee.
        bool playerAlive = Main.PlayerInfo != null && !Main.PlayerInfo.Destroyed;
        bool playerInRange = playerAlive &&
            Vector3.Distance(Main.PlayerInfo.position, transform.position) < Info.DistAlert;
        if (playerInRange && Info.Target != Main.PlayerInfo)
        {
            Info.Target = Main.PlayerInfo;
            Info.PathingStatus = PathingStatus.Pending;
        }
        else if (!playerInRange && Info.Target == Main.PlayerInfo &&
                 Vector3.Distance(Main.PlayerInfo.position, transform.position) > Info.DistDisengage)
        {
            Info.CancelTarget();
        }

        if (Info.Target == null)
        {
            if (Random.value > 0.5f)
                SetState<MobIdle>();
            return;
        }

        // Bite while hovering on the player.
        if (Vector3.Distance(Info.Target.position, transform.position) < Info.DistAttack)
        {
            Projectile.Spawn(transform.position, Info.Target.position, TouchProjectile, Info.targetHitboxType, Info);
        }
    }
}
