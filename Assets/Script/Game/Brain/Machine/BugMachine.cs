 
using UnityEngine;

public class BugMachine : GroundMobMachine
{
    protected override bool UsesDoorBash => true;

    private const float LatchRange = 1.5f; // how close to the player before latching
    private const int LatchCooldownTicks = 180; // ~3s after release before it can latch again
    private int _latchCooldown;

    public static Info CreateInfo()
    {
        return new BugInfo()
        {
            HealthMax = 15,
            DistAttack = 8,
            DistAlert = 10,
            PathJump = 2,
            PathAir = 6,
            SpeedGround = 14,
            SpeedAir = 10,
            JumpVelocity = 12,
        };
    }
    public override void OnStart()
    { 
        base.OnStart();

        AddState(new MobIdle());
        AddState(new MobChase());
        AddState(new MobStrafe());
        AddState(new MobRoam());
        AddState(new MobHit());
        AddState(new MobAttackPounce(5));
        AddState(new MobEscape());
    }

    public override void OnUpdate()
    { 
        if (_latchCooldown > 0) _latchCooldown--;

        if (Info is BugInfo bugInfo && bugInfo.LatchedPlayer != null)
        {
            LatchedUpdate(bugInfo);
            return;
        }

        UpdateAggro();

        if (!IsCurrentState<DefaultState>()) return;

        // Keep fleeing after a latch breaks until the cooldown clears.
        if (_latchCooldown > 0)
        {
            SetState<MobEscape>();
            return;
        }

        if (Info.Target == null)
        {
            switch (Random.Range(1, 6))
            {
                case 1:
                    SetState<MobRoam>();
                    break;
                case 2:
                case 3:
                    SetState<MobStrafe>();
                    break;
                default:
                    SetState<MobIdle>();
                    break;
            }
            return;
        }

        if (Vector3.Distance(Info.Target.position, transform.position) < Info.DistAttack)
        {
            if (Info.Target is PlayerInfo player && TryLatch(player)) return;

            if (Random.value < 0.2f)
                SetState<MobStrafe>();
            else
            {
                Info.AimPosition = Info.Target.position;
                SetState<MobAttackPounce>();
            }
        }
        else if (Info.PathingStatus == PathingStatus.Stuck)
        {
            SetState<MobRoam>();
        }
        else
        {
            SetState<MobChase>();
        }
    }

    /// <summary>Attempt to latch onto the player. Returns true if latched.</summary>
    private bool TryLatch(PlayerInfo player)
    {
        if (Vector3.Distance(player.position, transform.position) > LatchRange) return false;
        if (Info is not BugInfo bugInfo) return false;
        bugInfo.Latch(player);
        return true;
    }

    /// <summary>While latched, stick to the player's face.</summary>
    private void LatchedUpdate(BugInfo bugInfo)
    {
        PlayerInfo player = bugInfo.LatchedPlayer;
        if (player == null || player.Destroyed)
        {
            bugInfo.Release();
            return;
        }

        // Hug the player's face (just above their head).
        transform.position = player.position + Vector3.up * 1.6f;
        Info.Direction = Vector3.zero;
        Info.PathingStatus = PathingStatus.Reached;
    }

    /// <summary>Called when the bug is released — flee away before it can relatch.</summary>
    public void Panic()
    {
        if (Info is BugInfo bugInfo) bugInfo.Release();
        _latchCooldown = LatchCooldownTicks;
        SetState<MobEscape>();
    }
}