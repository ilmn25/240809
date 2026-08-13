using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>A mysterious visitor that appears near the end of the night. It is
/// passive and simply follows the player around. If the player attacks it, it
/// becomes enraged: it locks onto the attacker, kills them (very strong and
/// fast), then runs away and despawns. If left alone, it eventually wanders off
/// and leaves on its own.</summary>
public class VisitorMachine : GroundMobMachine
{
    private static readonly ProjectileInfo ClawProjectile = new ContactDamageProjectileInfo {
        Damage = 8,          // very strong — kills most players in a couple hits
        Knockback = 16,
        CritChance = 0.2f,
        Radius = 1.0f,
    };

    private const int LeaveDelay = 60 * 10;    // frames before it leaves if left alone (~10s)
    private const int FleeDelay = 60 * 3;      // frames it flees after killing (~3s)

    private int _leaveTimer;
    private int _fleeTimer;
    private bool _enraged;

    public static Info CreateInfo()
    {
        return new VisitorInfo()
        {
            HealthMax = 200,     // very tanky — hard to kill
            SpeedGround = 8,     // very fast
            SpeedAir = 10,
            DistAttack = 2,
            DistFollow = 4,
            DistAlert = 12,
            DistDisengage = 40,
            DistRoam = 6,
            IsNPC = true,
            CharSprite = ID.Chito, // reuse the player's sprite
        };
    }

    public override void OnStart()
    {
        base.OnStart();

        AddState(new MobIdle());
        AddState(new MobChase());
        AddState(new MobChaseAction());
        AddState(new MobRoam());
        AddState(new MobHit());
        AddState(new MobAttackStopSwing(ClawProjectile));
        AddState(new MobEscape());
        AddState(new EquipSelectState());
    }

    public override void OnUpdate()
    {
        if (!IsCurrentState<DefaultState>()) return;

        // Enraged: chase and kill the attacker, then flee and despawn.
        if (_enraged)
        {
            if (Info.Target != null)
            {
                if (Vector3.Distance(Info.Target.position, transform.position) < Info.DistAttack)
                {
                    Info.AimPosition = Info.Target.position;
                    SetState<MobAttackStopSwing>();
                }
                else
                    SetState<MobChase>();
            }
            else
            {
                // Target killed or fled — run away, then despawn shortly after.
                if (++_fleeTimer >= FleeDelay)
                    Leave();
                else
                    SetState<MobEscape>();
            }
            return;
        }

        // Passive: follow the player. If left alone long enough, wander off.
        if (Main.PlayerInfo != null && !Main.PlayerInfo.Destroyed)
        {
            Info.Target = Main.PlayerInfo;
            Info.ActionType = IActionType.Follow;
            SetState<MobChaseAction>();
            _leaveTimer = 0;
        }
        else if (++_leaveTimer >= LeaveDelay)
        {
            // No player around — leave.
            Leave();
        }
    }

    /// <summary>Called when the visitor is hit — it locks onto the attacker and
    /// becomes enraged.</summary>
    public void Enrage(Info attacker)
    {
        if (_enraged) return;
        _enraged = true;
        Info.Target = attacker;
        Info.PathingStatus = PathingStatus.Pending;
        SetState<MobChase>();
    }

    /// <summary>Removes the visitor and lets the spawner know it's gone.</summary>
    private void Leave()
    {
        VisitorSpawner.MarkLeft();
        Info.Destroy();
        Unload();
    }
}
