 
using Unity.VisualScripting;
using UnityEngine; 

public class HunterMachine : EntityMachine, IHitBox
{   
    private int _ammo;
    private const int AmmoMax = 5;
    public override void OnStart()
    {
        _ammo = AmmoMax; 
        AddModule(new MobStatusModule
        {
            HitboxType = HitboxType.Enemy,
            HealthMax = 100,
            Defense = 1,
            Equipment = Item.GetItem("pistol"),
            DistAttack = 16,
            HurtSfx = "npc_hurt", 
            DeathSfx = "player_die",
            SpeedGround = 2.8f
        });
        AddModule(new GroundMovementModule());
        AddModule(new GroundPathingModule());
        AddModule(new GroundAnimationModule()); 
        AddModule(new MobSpriteCullModule());
        AddModule(new MobSpriteOrbitModule());
        
        AddState(new DefaultState(), true);
        AddState(new MobIdle());
        AddState(new MobChase());
        AddState(new MobRoam());
        AddState(new MobAttackReload());
        AddState(new MobAttackShoot());
    } 
    
    public override void OnUpdate()
    {
        HandleInput();

        if (IsCurrentState<DefaultState>())
        {
            if (Status.Target)
            {
                if (Vector3.Distance(Status.Target.transform.position, transform.position) < Status.DistAttack 
                    // Physics.Raycast(transform.position, toTarget.normalized, out RaycastHit hit, distanceToTarget, Game.MaskEntity) &&
                    // hit.collider.transform == Status.Target &&
                    // hit.distance <= 1f
                    )
                {
                    if (_ammo != 0)
                    {
                        _ammo--;
                        SetState<MobAttackShoot>();
                    }
                    else
                    {
                        SetState<MobAttackReload>();
                        _ammo = AmmoMax;
                    }
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