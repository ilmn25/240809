 
using UnityEngine; 

public class HunterMachine : MobMachine
{   
    public static Info CreateInfo()
    {
        return new EnemyInfo()
        {
            HitboxType = HitboxType.Enemy,
            TargetHitboxType = HitboxType.Friendly,
            HealthMax = 12,
            Defense = 1, 
            DistAttack = 18,
            HurtSfx = "npc_hurt", 
            DeathSfx = "player_die",
            SpeedGround = 2.8f,
            EquipmentId = "pistol"
        }; 
    }
    private int _ammo;
    private const int AmmoMax = 5; 

    public override void OnStart()
    {
        _ammo = AmmoMax;  
        AddModule(new GroundMovementModule());
        AddModule(new GroundPathingModule());
        AddModule(new GroundAnimationModule()); 
        AddModule(new MobSpriteCullModule()); 
        AddModule(new SpriteOrbitModule());
        
        AddState(new MobIdle());
        AddState(new MobChaseAim());
        AddState(new MobRoam());
        AddState(new MobAttackReload());
        AddState(new MobAttackShoot());
        AddState(new EquipSelectState()); 
        Info.SetEquipment(Info.EquipmentId);
    } 
    
    public override void OnUpdate()
    {
        HandleInput();
        
        if (IsCurrentState<DefaultState>())
        {
            if (Info.Target)
            {
                if (InRangeAndVisible())
                {
                    if (_ammo != 0)
                    {
                        _ammo--;
                        Info.AimPosition = Info.Target.transform.position + 0.3f * Vector3.up;
                        SetState<MobAttackShoot>();
                    }
                    else
                    {
                        SetState<MobAttackReload>();
                        _ammo = AmmoMax;
                    }
                } 
                else if (Info.PathingStatus == PathingStatus.Stuck)
                {
                    SetState<MobRoam>();
                }
                else
                {
                    SetState<MobChaseAim>();
                }
            }
            else
            { 
                if (Random.value > 0.5f)
                    SetState<MobRoam>();
                else
                    SetState<MobIdle>();
            }


            bool InRangeAndVisible()
            {
                Vector3 origin = transform.position + Vector3.up * 0.3f; 
                float distance = Vector3.Distance(origin, Info.Target.transform.position);

                // Debug.DrawRay(origin, direction * distance, Color.red, 0.1f); // Lasts 0.1 seconds

                if (distance > Info.DistAttack) return false;

                if (Physics.Raycast(origin, (Info.Target.transform.position - origin).normalized,
                        out RaycastHit hit, distance, Game.MaskMap))
                {
                    return hit.distance >= distance - 0.2f;
                }
        
                return true;
            }
        }
    }
    
    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            Info.Target = Game.Player.transform;
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