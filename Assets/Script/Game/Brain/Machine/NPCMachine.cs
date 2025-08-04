 
using UnityEngine;


[System.Serializable]
public class NPCCED : ChunkEntityData
{
    public string npcStatus = "idle";
}

public class NPCMachine : EntityMachine , IActionSecondary, IHitBox
{ 
    public override void OnInitialize()
    {
        AddState(new NPCState()); 
        AddModule(new NPCStatusModule(HitboxType.Enemy,100,1));
        AddModule(new NPCMovementModule());
        AddModule(new NPCPathingModule());
        AddModule(new NPCAnimationModule()); 
        AddModule(new SpriteCullModule()); 
        AddModule(new SpriteOrbitModule()); 
    }

    public void OnActionSecondary()
    {
        GetState<NPCState>().SetState<CharTalk>();
    }   
    
    public override void UpdateEntityData()
    {
        switch (GetState<NPCState>().GetCurrentStateType())
        {
            case { } t when t == typeof(NPCIdle):  
                ((NPCCED) entityData).npcStatus = "idle";
                break;
            case { } t when t == typeof(NPCChase):  
                ((NPCCED) entityData).npcStatus = "chase";
                break;
            default: 
                ((NPCCED) entityData).npcStatus = "roam";
                break;
        } 
    }

    // public void OnDrawGizmos()
    // {
    //     GetModule<NPCPathingModule>().DrawGizmos();
    // }
}