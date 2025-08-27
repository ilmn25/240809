 
using UnityEngine;

 

public class BugMachine : MobMachine
{   
    public static Info CreateInfo()
    {
        return new EnemyInfo()
        {
            HealthMax = 15,
            DistAttack = 8,
            PathJump = 2,
            PathAir = 6,
            SpeedGround = 14,
            SpeedAir = 10,
            JumpVelocity = 12,
        };
    }
    public override void OnStart()
    { 
        AddModule(new GroundMovementModule());
        AddModule(new GroundPathingModule());
        AddModule(new GroundAnimationModule());
        AddModule(new MobSpriteCullModule());
        AddModule(new SpriteOrbitModule());

        AddState(new MobIdle());
        AddState(new MobChase());
        AddState(new MobStrafe());
        AddState(new MobRoam());
        AddState(new MobHit());
        AddState(new MobAttackPounce(5));
    }

    public override void OnUpdate()
    { 
        if (IsCurrentState<DefaultState>())
        {
            if (Info.Target != null)
            {
                if (Vector3.Distance(Info.Target.position, transform.position) < Info.DistAttack)
                {
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
    public void OnDrawGizmos()
    {
        if (Camera.current != Camera.main)
            return;

        GetModule<GroundPathingModule>().DrawGizmos();
    }
}