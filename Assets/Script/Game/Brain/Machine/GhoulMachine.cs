 
using UnityEngine;

public class MobMachine : EntityMachine, IActionPrimaryAttack
{
    public new MobInfo Info => GetModule<MobInfo>();
    public override void OnSetup()
    {
        transform.Find("sprite").Find("char").GetComponent<SpriteRenderer>().sprite =
            Cache.LoadSprite("character/" + Info.stringID);
    }
}

public class GhoulMachine : MobMachine, IActionSecondaryInteract
{   
    public static Info CreateInfo()
    { 
        return new EnemyInfo()
        {
            HitboxType = HitboxType.Enemy,
            TargetHitboxType = HitboxType.Friendly,
            HealthMax = 16,
            Defense = 1, 
            DistAttack = 1,
            HurtSfx = "npc_hurt",
            DeathSfx = "player_die",
            DistRoam = 7 
        };  
    }
    public override void OnStart()
    {
         
        Dialogue dialogue = new Dialogue();
        dialogue.Lines.Add("hello");
        dialogue.Lines.Add("are u buying or not");
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
        AddState(new EquipSelectState());
        AddState(new DialogueState(dialogue)); 
        Info.SetEquipment("sword");
    }

    public void OnActionSecondary(Info info)
    {
        if (Info.Target != null) return;
        SetState<DialogueState>();
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
        {
            Info.Target = (MobInfo) Game.PlayerInfo;
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