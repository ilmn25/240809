 
using UnityEngine;

public class HarpyMachine : MobMachine
{   
    public static Info CreateInfo()
    { 
        return new EnemyInfo()
        {
            HealthMax = 16,
            DistAttack = 1,
            DistRoam = 7,
            PathJump = 10,
            PathAir = -1,
            SpeedAir = 7,
            JumpVelocity = 7,
            CanFly = true,
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
        AddState(new MobRoam());
        AddState(new MobEvade());
        AddState(new MobHit());
        AddState(new MobAttackSwing());
        AddState(new EquipSelectState());
        Info.SetEquipment(new ItemSlot(ID.SteelSword));
    }
 
    public override void OnUpdate()
    {
        HandleInput();

        if (IsCurrentState<DefaultState>())
        {
            if (Info.Target != null)
            {
                if (Vector3.Distance(Info.Target.position, transform.position) < Info.DistAttack)
                {
                    if (Random.value < 0.7f)
                    {
                        Info.AimPosition = Info.Target.position;
                        Attack();
                    } 
                    else
                        SetState<MobEvade>();
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
        {
            Info.Target = Game.PlayerInfo;
            Info.PathingStatus = PathingStatus.Reached; 
            SetState<DefaultState>();
        } 
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