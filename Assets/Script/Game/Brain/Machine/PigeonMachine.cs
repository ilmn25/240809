using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>A pigeon that flies above the player, drops a poop projectile on them,
/// then flies away. It's a nuisance enemy — low health, no melee, just the poop.</summary>
public class PigeonMachine : GroundMobMachine
{
    private static readonly PoopProjectileInfo PoopProjectile = new PoopProjectileInfo {
        Damage = 2,
        Knockback = 4,
        Radius = 0.6f,
        Speed = 8f,
        Sprite = ID.BirdShit,
        Scale = 0.6f,
    };

    private const float HoverHeight = 8f;    // how high above the player it flies
    private const int PoopDelay = 120;       // frames before it poops (~2s)
    private const int FleeDelay = 90;        // frames it flees after pooping (~1.5s)

    private int _timer;
    private bool _pooped;

    public static Info CreateInfo()
    {
        return new EnemyInfo()
        {
            HealthMax = 8,
            DistAlert = 20,
            DistDisengage = 30,
            DistRoam = 6,
            PathJump = 10,
            PathAir = -1,
            SpeedGround = 6,
            SpeedAir = 8,
            JumpVelocity = 7,
            CanFly = true,
        };
    }

    public override void OnStart()
    {
        base.OnStart();

        AddState(new MobIdle());
        AddState(new MobChase());
        AddState(new MobRoam());
        AddState(new MobHit());
        AddState(new MobEscape());
        AddState(new EquipSelectState());
    }

    public override void OnUpdate()
    {
        if (IsCurrentState<DefaultState>())
        {
            // Fly above the player.
            if (Main.PlayerInfo != null && !Main.PlayerInfo.Destroyed)
            {
                Vector3 above = Main.PlayerInfo.position + Vector3.up * HoverHeight;
                transform.position = Vector3.MoveTowards(transform.position, above, Info.SpeedAir * Time.deltaTime);

                // After a short hover, drop the poop and flee.
                if (++_timer >= PoopDelay)
                {
                    _timer = 0;
                    DropPoop();
                    _pooped = true;
                }
            }
            else if (++_timer >= FleeDelay)
            {
                // No player — just leave.
                Info.Destroy();
                Unload();
            }
        }
        else if (_pooped && IsCurrentState<MobEscape>())
        {
            // Flee for a bit, then despawn.
            if (++_timer >= FleeDelay)
            {
                Info.Destroy();
                Unload();
            }
        }
    }

    /// <summary>Drop a poop projectile straight down onto the player below.</summary>
    private void DropPoop()
    {
        Vector3 origin = transform.position;
        Vector3 dest = origin + Vector3.down * HoverHeight;
        Projectile.Spawn(origin, dest, PoopProjectile, Info.targetHitboxType, Info);
        Audio.PlaySFX(SfxID.HitStone);
        SetState<MobEscape>();
    }
}
