 
using UnityEngine;

public class SheepMachine : MobMachine, IActionSecondaryInteract
{   
    public static Info CreateInfo()
    { 
        return new EnemyInfo()
        {
            HealthMax = 16,
            Defense = 1, 
            SpeedGround = 4,
            SpeedAir = 7,
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
        
        Dialogue dialogue = new Dialogue(); 
        dialogue.Lines.Add("baaa"); 
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
                    SetState<MobEscape>();
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