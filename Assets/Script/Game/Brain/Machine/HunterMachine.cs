 
using Unity.VisualScripting;
using UnityEngine; 

public class HunterMachine : EntityMachine, IHitBox
{ 
    public override void OnInitialize()
    {
        AddModule(new MobStatusModule
        {
            HitboxType = HitboxType.Enemy,
            HealthMax = 100,
            Defense = 1,
            Equipment = Item.GetItem("minigun"),
            AttackDistance = 5,
            HurtSfx = "npc_hurt", 
            DeathSfx = "player_die"
        });
        AddModule(new GroundMovementModule());
        AddModule(new GroundPathingModule());
        AddModule(new GroundAnimationModule()); 
        AddModule(new MobSpriteCullModule()); 
        AddModule(new MobSpriteOrbitModule()); 
        AddState(new HunterState());
    }

    public void OnDrawGizmos()
    {
        GetModule<GroundPathingModule>().DrawGizmos();
    }
}

public class HunterState : MobState
{
    private int _ammo;
    private const int AmmoMax = 5;

    public override void OnEnterState()
    {
        AddState(new StateEmpty(), true);
        AddState(new MobIdle());
        AddState(new MobChase());
        AddState(new MobRoam());
        AddState(new MobAttackShoot());
        _ammo = AmmoMax;
    } 
    public override void OnUpdateState()
    {
        HandleInput();

        if (IsCurrentState<StateEmpty>())
        {
            if (Status.Target)
            {
                if (Vector3.Distance(Status.Target.transform.position, Machine.transform.position) <
                    Status.AttackDistance)
                {
                    if (_ammo != 0)
                    {
                        _ammo--;
                        SetState<MobAttack>();
                    }
                    else
                    {
                        SetState<MobReload>();
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
            Machine.transform.position = Game.Player.transform.position;
    }
} 