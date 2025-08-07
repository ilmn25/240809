 
using UnityEngine;

public class BugMachine : EntityMachine, IHitBox
{   
    public override void OnStart()
    {
        AddModule(new MobStatusModule
        {
            HitboxType = HitboxType.Enemy,
            HealthMax = 100,
            Defense = 1,
            DistAttack = 8,
            HurtSfx = "npc_hurt",
            DeathSfx = "player_die",
            PathJump = 2,
            PathAir = 6
        });
        AddModule(new GroundMovementModule(speed: 14, speedAir: 10, jumpVelocity: 12f));
        AddModule(new GroundPathingModule());
        AddModule(new GroundAnimationModule());
        AddModule(new MobSpriteCullModule());
        AddModule(new MobSpriteOrbitModule());

        AddState(new DefaultState(), true);
        AddState(new MobIdle());
        AddState(new MobChase());
        AddState(new MobRoam());
        AddState(new MobAttackPounce());
    }

    public override void OnUpdate()
    {
        HandleInput();

        if (IsCurrentState<DefaultState>())
        {
            if (Status.Target)
            {
                if (Vector3.Distance(Status.Target.transform.position, transform.position) < Status.DistAttack)
                {
                    if (Random.value < 0.5f)
                        SetState<MobRoam>();
                    else
                        SetState<MobAttackPounce>();
                }
                else if (Status.PathingStatus == PathingStatus.Stuck)
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
                if (Random.value > 0.5f)
                    SetState<MobRoam>();
                else
                    SetState<MobIdle>();
            }
        }
    }
    
    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Y))
            Status.Target = Game.Player.transform;
        else if (Input.GetKeyDown(KeyCode.T))
            Status.Target = null;
        else if (Input.GetKeyDown(KeyCode.U))
            transform.position = Game.Player.transform.position;
    }

    public void OnDrawGizmos()
    {
        GetModule<GroundPathingModule>().DrawGizmos();
    }
} 