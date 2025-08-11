 
using UnityEngine;

[System.Serializable]
public class NPCCED : ChunkEntityData
{
    public string npcStatus = "idle";
}


public class MobMachine : EntityMachine, IHitBox
{
    public override void OnSetup()
    {
        transform.Find("sprite").Find("char").GetComponent<SpriteRenderer>().sprite =
            Cache.LoadSprite("character/" + entityData.stringID);
    }
}

public class GhoulMachine : MobMachine, IActionSecondary
{   
    public override void OnStart()
    {
        Dialogue dialogue = new Dialogue();
        dialogue.Lines.Add("hello");
        dialogue.Lines.Add("are u buying or not");
        AddModule(new EnemyInfo()
        {
            HitboxType = HitboxType.Enemy,
            TargetHitboxType = HitboxType.Friendly,
            HealthMax = 100,
            Defense = 1,
            Equipment = Item.GetItem("sword"),
            DistAttack = 1,
            HurtSfx = "npc_hurt",
            DeathSfx = "player_die",
            DistRoam = 7,
        });
        AddModule(new GroundMovementModule());
        AddModule(new GroundPathingModule());
        AddModule(new GroundAnimationModule());
        AddModule(new MobSpriteCullModule());
        AddModule(new SpriteOrbitModule());

        AddState(new MobIdle());
        AddState(new MobChase());
        AddState(new MobRoam());
        AddState(new MobEvade());
        AddState(new MobAttackSwing());
        AddState(new DialogueState(dialogue));
    }

    public void OnActionSecondary()
    {
        if (Info.Target) return;
        SetState<DialogueState>();
    }
    public override void OnUpdate()
    {
        HandleInput();

        if (IsCurrentState<DefaultState>())
        {
            if (Info.Target)
            {
                if (Vector3.Distance(Info.Target.transform.position, transform.position) < Info.DistAttack)
                {
                    if (Random.value < 0.7f)
                    {
                        Info.AimPosition = Info.Target.transform.position;
                        SetState<MobAttackSwing>();
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
            Info.Target = Game.Player.transform;
        else if (Input.GetKeyDown(KeyCode.T))
            Info.Target = null;
        else if (Input.GetKeyDown(KeyCode.U))
            transform.position = Game.Player.transform.position;
    }

    public void OnDrawGizmos()
    {
        GetModule<GroundPathingModule>().DrawGizmos();
    }
 
} 