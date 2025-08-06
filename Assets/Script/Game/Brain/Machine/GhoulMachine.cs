 
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
        AddState(new GhoulState()); 
        AddModule(new MobStatusModule(HitboxType.Enemy,100,1, "npc_hurt", "player_death"));
        AddModule(new GroundMovementModule());
        AddModule(new GroundPathingModule());
        AddModule(new GroundAnimationModule()); 
        AddModule(new SpriteCullModule()); 
        AddModule(new SpriteOrbitModule()); 
    }

    public void OnDrawGizmos()
    {
        GetModule<GroundPathingModule>().DrawGizmos();
    }
}