 
using System.Collections.Generic;
using UnityEngine;

public class GhoulMachine : MobMachine, IActionSecondaryInteract
{   
    public static Info CreateInfo()
    { 
        return new EnemyInfo()
        {
            HealthMax = 16,
            DistAttack = 2,
            DistRoam = 7 
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
        
        var dialogue = new Dialogue
        {
            Text = "go away femboy",
            Sprite = Cache.LoadSprite("Sprite/Illu"),
            Next = new Dictionary<string, Dialogue>
            {
                [""] = new() {
                    Text = "......",
                    Sprite = Cache.LoadSprite("Sprite/Illu"),
                    Next = new Dictionary<string, Dialogue>
                    {
                        [""] = new() { 
                            Text = "i said go away",
                            Sprite = Cache.LoadSprite("Sprite/Illu"),
                        }
                    }
                }
            }
        };
        AddState(new DialogueState(dialogue)); 
        
        Info.SetEquipment(new ItemSlot(ID.SteelSword));
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
                    if (Random.value < 0.9f)
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
     
    public void OnDrawGizmos()
    {
        if (Camera.current != Camera.main)
            return;

        GetModule<GroundPathingModule>().DrawGizmos();
    }
 
} 