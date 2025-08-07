 
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class NPCCED : ChunkEntityData
{
    public string npcStatus = "idle";
}


public class GhoulMachine : EntityMachine, IHitBox
{  

    public override void OnStart()
    {
        AddModule(new MobStatusModule
        {
            HitboxType = HitboxType.Enemy,
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
        AddModule(new MobSpriteOrbitModule());

        AddState(new DefaultState(), true);
        AddState(new MobIdle());
        AddState(new MobChase());
        AddState(new MobRoam());
        AddState(new MobEvade());
        AddState(new MobAttackSwing());
    }

    public override void OnUpdate()
    {
        HandleInput();

        if (IsCurrentState<DefaultState>())
        {
            if (Status.Target)
            {
                if (Vector3.Distance(Status.Target.transform.position, transform.position) < Status.DistAttack)
                {
                    if (Random.value < 0.7f)
                        SetState<MobAttackSwing>();
                    else
                        SetState<MobEvade>();
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