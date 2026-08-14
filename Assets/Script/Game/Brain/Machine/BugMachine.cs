 
using UnityEngine;

public class BugMachine : GroundMobMachine
{
    protected override bool UsesDoorBash => true;

    private const float LatchRange = 1.5f; // how close to the player before latching

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
    }

    public override void OnUpdate()
    { 
        // While latched, hug the player's face and don't act on our own.
        if (Info is BugInfo bugInfo && bugInfo.LatchedPlayer != null)
        {
            LatchedUpdate(bugInfo);
            return;
        }

        // Aggro on sight so the bug attacks without needing to be hit first.
        UpdateAggro();

        if (IsCurrentState<DefaultState>())
        {
            if (Info.Target != null)
            {
                if (Vector3.Distance(Info.Target.position, transform.position) < Info.DistAttack)
                {
                    // Try to latch onto the player when close enough.
                    if (Info.Target is PlayerInfo player && TryLatch(player))
                        return;
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
            else
            {
                switch (Random.Range(1,6))
                {
                    case 1:
                        SetState<MobRoam>();
                        break;
                    case 2:
                    case 3:
                        SetState<MobStrafe>();
                        break;
                    case 4:
                    case 5: 
                        SetState<MobIdle>();
                        break;
                } 
            }
        }
    }

    /// <summary>Attempt to latch onto the player. Returns true if latched.</summary>
    private bool TryLatch(PlayerInfo player)
    {
        if (Vector3.Distance(player.position, transform.position) > LatchRange) return false;
        if (Info is BugInfo bugInfo)
        {
            bugInfo.Latch(player);
            return true;
        }
        return false;
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

    /// <summary>Called when the bug is released — go into a strafe panic.</summary>
    public void Panic()
    {
        if (Info is BugInfo bugInfo) bugInfo.Release();
        SetState<MobStrafe>();
    }
}