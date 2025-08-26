using UnityEditor;
using UnityEngine;

public class SlimeMachine : MobMachine
{   
    private readonly ProjectileInfo _projectileInfo = new ContactDamageProjectileInfo {
        Damage = 1,
        Knockback = 10,
        CritChance = 0.1f,
        Radius = 0.7f,
    };
    
    public static Info CreateInfo()
    {
        return new EnemyInfo()
        {
            HealthMax = 16,
            Defense = 1,
            DistAttack = 3,
            PathJump = 3,
            PathAir = 6,
            DistRoam = 15,
            DistStrafe = 3,
            SpeedGround = 0,
            SpeedLogic = 2,
            SpeedAir = 14,
            JumpVelocity = 15,
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
                switch (Random.Range(1,3))
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

    public void OnDrawGizmos()
    {
        if (Camera.current != Camera.main)
            return;

        GetModule<GroundPathingModule>().DrawGizmos();
    }
}