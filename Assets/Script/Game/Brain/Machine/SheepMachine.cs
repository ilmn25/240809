 
using UnityEngine;

public class SheepMachine : MobMachine, IActionSecondaryInteract
{   
    public static Info CreateInfo()
    { 
        return new EnemyInfo()
        {
            HealthMax = 16,
            SpeedGround = 7,
            SpeedAir = 8,
            PathAir = 3,
            DistAttack = 7,
            DistRoam = 3 
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
        AddState(new MobRoam());
        AddState(new MobEscape());
        AddState(new MobHit());
        AddState(new EquipSelectState());
        
        Dialogue dialogue = new Dialogue
        {
            Text = "baaaa", 
        };
        AddState(new DialogueState(dialogue)); 
    }

    public void OnActionSecondary(Info info)
    {
        if (Info.Target != null) return;
        SetState<DialogueState>();
    }
    public override void OnUpdate()
    { 

        if (IsCurrentState<DefaultState>())
        {
            if (Info.Target != null)
            {
                if (Vector3.Distance(Info.Target.position, transform.position) < Info.DistAttack)
                {
                    if (Random.value < 0.8f)
                    {
                        SetState<MobEscape>();
                    } 
                    else
                        SetState<MobRoam>(); 
                } 
                else
                {
                    SetState<MobRoam>();
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
     
    public void OnDrawGizmos()
    {
        if (Camera.current != Camera.main)
            return;

        GetModule<GroundPathingModule>().DrawGizmos();
    }
 
} 