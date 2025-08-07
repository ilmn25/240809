 
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class NPCCED : ChunkEntityData
{
    public string npcStatus = "idle";
}


public class GhoulMachine : EntityMachine, IHitBox
{ 
    public override void OnInitialize()
    {    
        AddModule(new MobStatusModule
        {
            HitboxType = HitboxType.Enemy,
            HealthMax = 100,
            Defense = 1,
            Equipment = Item.GetItem("axe"),
            AttackDistance = 2,
            HurtSfx = "npc_hurt", 
            DeathSfx = "player_die"
        });
        AddModule(new GroundMovementModule());
        AddModule(new GroundPathingModule());
        AddModule(new GroundAnimationModule()); 
        AddModule(new MobSpriteCullModule()); 
        AddModule(new MobSpriteOrbitModule()); 
        AddState(new MobGroundState()); 
        GetState<MobGroundState>().AddState(new MobAttackPounce());
    }

    public void OnDrawGizmos()
    {
        GetModule<GroundPathingModule>().DrawGizmos();
    }
}