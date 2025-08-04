 
using UnityEngine;


[System.Serializable]
public class NPCCED : ChunkEntityData
{
    public string npcStatus = "idle";
}

public class NPCMachine : EntityMachine , IActionSecondary, IHitBox
{ 
    private float _health = 100;
    public override void OnInitialize()
    {
        AddState(new NPCState()); 
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
    public void OnHit(Projectile projectile)
    {
        if (projectile.Target == ProjectileTarget.Ally) return;
        GetState<NPCState>().SetState<NPCChase>();
        GetModule<NPCMovementModule>().KnockBack(projectile.transform.position, projectile.Info.Knockback, true);
        _health -= projectile.Info.GetDamage();  
        if (_health <= 0)
        {
            Audio.PlaySFX("player_die", 0.4f);
            Entity.SpawnItem("sand", Vector3Int.FloorToInt(transform.position));
            Delete();
        }
        else
        {
            Audio.PlaySFX("npc_hurt", 0.8f);
        }
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

    public void OnDrawGizmos()
    {
        GetModule<NPCPathingModule>().DrawGizmos();
    }
}