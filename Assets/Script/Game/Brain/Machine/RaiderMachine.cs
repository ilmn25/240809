 
using System.Collections.Generic;
using UnityEngine;

public class RaiderMachine : GroundMobMachine, IActionSecondaryInteract
{
    protected override bool UsesDoorBash => true;

    public static Info CreateInfo()
    { 
        return new EnemyInfo()
        {
            HealthMax = 16,
            DistRoam = 7 
        };  
    }
    public override void OnStart()
    {
        base.OnStart();

        AddState(new MobIdle());
        AddState(new MobChase());
        AddState(new MobRoam());
        AddState(new MobEvade());
        AddState(new MobHit());
        AddState(new MobAttackSwing());
        AddState(new EquipSelectState());
        
        var dialogue = new Dialogue
        {
            Text = "You shouldn't be here.",
            Sprite = Cache.LoadSprite("Sprite/Illu"),
            Next = new Dictionary<string, Dialogue>
            {
                [""] = new() {
                    Text = "This is our turf. Turn around and leave.",
                    Sprite = Cache.LoadSprite("Sprite/Illu"),
                    Next = new Dictionary<string, Dialogue>
                    {
                        [""] = new() { 
                            Text = "Or don't. I could use the exercise.",
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
        UpdateAggro();

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
     
} 