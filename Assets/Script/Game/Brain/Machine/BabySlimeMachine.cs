using UnityEngine;

public class BabySlimeMachine : MobMachine
{
    public static Info CreateInfo()
    {
        return new EnemyInfo()
        {
            HealthMax = 8,
            Defense = 0,
            DistAttack = 2,
            PathJump = 3,
            PathAir = 6,
            DistRoam = 12,
            DistStrafe = 3,
            SpeedGround = 0,
            SpeedLogic = 2,
            SpeedAir = 12,
            JumpVelocity = 12,
            DecelerationTime = 0,
            AccelerationTime = 0.1f,
            NormalSkipAmount = 3,
            mustLandFirst = true,
            MaxStuckCount = 700,
            PointLostDistance = 7,
        };
    }

    public override void OnStart()
    {
        AddModule(new SlimeMovementModule());
        AddModule(new GroundPathingModule());
        AddModule(new MobSpriteCullModule());
        AddModule(new SpriteOrbitModule());
        AddModule(new DoorBashModule());

        AddState(new MobIdle());
        AddState(new MobChase());
        AddState(new MobRoam());
        AddState(new MobHit());
        AddState(new MobAttackPounce(1));
    }

    public override void OnUpdate()
    {
        if (IsCurrentState<DefaultState>())
        {
            if (Info.Target != null)
            {
                if (Vector3.Distance(Info.Target.position, transform.position) < Info.DistAttack)
                {
                    Info.AimPosition = Info.Target.position;
                    SetState<MobAttackPounce>();
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
                switch (Random.Range(1, 3))
                {
                    case 1:
                        SetState<MobRoam>();
                        break;
                    case 2:
                        SetState<MobIdle>();
                        break;
                }
            }
        }
    }
}
