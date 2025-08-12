 
using UnityEngine;

public class BugMachine : MobMachine
{   
    public override void OnStart()
    {
        AddModule(new EnemyInfo()
        {
            HitboxType = HitboxType.Enemy,
            TargetHitboxType = HitboxType.Friendly,
            HealthMax = 100,
            Defense = 1,
            DistAttack = 8,
            HurtSfx = "npc_hurt",
            DeathSfx = "player_die",
            PathJump = 2,
            PathAir = 6,
            SpeedGround = 14,
            SpeedAir = 10,
            JumpVelocity = 12,
        });
        AddModule(new GroundMovementModule());
        AddModule(new GroundPathingModule());
        AddModule(new GroundAnimationModule());
        AddModule(new MobSpriteCullModule());
        AddModule(new SpriteOrbitModule());

        AddState(new MobIdle());
        AddState(new MobChase());
        AddState(new MobStrafe());
        AddState(new MobRoam());
        AddState(new MobAttackPounce());
    }

    public override void OnUpdate()
    {
        HandleInput();

        if (IsCurrentState<DefaultState>())
        {
            if (Info.Target)
            {
                if (Vector3.Distance(Info.Target.transform.position, transform.position) < Info.DistAttack)
                {
                    if (Random.value < 0.2f)
                        SetState<MobStrafe>();
                    else
                    {
                        Info.AimPosition = Info.Target.transform.position;
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
    
    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Y))
            Info.Target = Game.Player.transform;
        else if (Input.GetKeyDown(KeyCode.T))
            Info.Target = null;
        else if (Input.GetKeyDown(KeyCode.U))
            transform.position = Game.Player.transform.position;
    }

    public void OnDrawGizmos()
    {
        if (Camera.current != Camera.main)
            return;

        GetModule<GroundPathingModule>().DrawGizmos();
    }
} 