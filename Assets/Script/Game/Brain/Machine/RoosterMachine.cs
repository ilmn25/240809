using UnityEngine;

public class RoosterMachine : AnimalMachine
{
    // A rooster tolerates the player getting close for a little while, then pecks them.
    private const int AggroDelay = 180;   // frames the player must linger (~3s at 60 fps)
    private const int PounceCount = 2;    // peck hops per attack

    private int _nearTimer;

    protected override string DialogueText => "cock-a-doodle-doo!";

    public static Info CreateInfo()
    {
        return new PassiveInfo()
        {
            HealthMax = 16,
            SpeedGround = 7,
            SpeedAir = 8,
            PathAir = 3,
            DistAttack = 3,
            DistAlert = 4,
            DistRoam = 3
        };
    }

    public override void OnStart()
    {
        base.OnStart();

        AddState(new MobChase());
        AddState(new MobAttackPounce(PounceCount));
    }

    public override void OnUpdate()
    {
        bool playerAlive = Main.PlayerInfo != null && !Main.PlayerInfo.Destroyed;
        bool playerNear = playerAlive &&
            Vector3.Distance(Main.PlayerInfo.position, transform.position) < Info.DistAlert;

        if (playerNear)
        {
            // Attack immediately if the player attacked us (target already set by OnHit),
            // otherwise attack if the player lingers too close for too long.
            if (Info.Target is PlayerInfo ||
                (++_nearTimer >= AggroDelay && Info.Target is not PlayerInfo))
            {
                Chase(Main.PlayerInfo);
                return;
            }
        }
        else
        {
            _nearTimer = 0;
            // Player stepped away — calm down once they're well out of reach.
            if (Info.Target is PlayerInfo &&
                (!playerAlive ||
                 Vector3.Distance(Main.PlayerInfo.position, transform.position) > Info.DistDisengage))
            {
                Info.CancelTarget();
            }
        }

        if (!IsCurrentState<DefaultState>()) return;

        if (Info.Target != null)
        {
            if (Vector3.Distance(Info.Target.position, transform.position) < Info.DistAttack)
            {
                if (Random.value < 0.8f)
                {
                    Info.AimPosition = Info.Target.position;
                    SetState<MobAttackPounce>();
                }
                else
                    SetState<MobRoam>();
            }
            else if (Info.PathingStatus == PathingStatus.Stuck)
                SetState<MobRoam>();
            else
                SetState<MobChase>();
        }
        else
        {
            if (Random.value > 0.5f)
                SetState<MobRoam>();
            else
                SetState<MobIdle>();
        }
    }
}
