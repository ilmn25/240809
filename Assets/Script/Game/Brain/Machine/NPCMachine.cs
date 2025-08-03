 
using UnityEngine;


[System.Serializable]
public class NPCCED : ChunkEntityData
{
    public string npcStatus = "idle";
}

public class NPCMachine : EntityMachine , IActionSecondary, IActionPrimary
{ 
    int health = 100;
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
    public void OnActionPrimary()
    {
        GetModule<NPCMovementModule>().KnockBack(Game.Player.transform.position, 12, true);
        health -= Inventory.CurrentItemData.Damage;
        if (health <= 0)
        {
            Audio.PlaySFX("player_die", 0.8f);
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

public class NPCState : State
{ 
    public override void OnEnterState()
    {
        string status = ((NPCCED)((EntityMachine)Machine).entityData).npcStatus;
        AddState(new NPCIdle(), status == "idle");
        AddState(new NPCChase(), status == "chase");
        AddState(new NPCRoam(),status == "roam");
        Dialogue dialogue = new Dialogue();
        dialogue.Lines.Add("when i was in primary school");
        dialogue.Lines.Add("i used to piss out the bathroom window off the building for fun");
        dialogue.Lines.Add("but one time my mom caught me because she saw the piss stream from the kitchen window");
        dialogue.Lines.Add("after that");
        dialogue.Lines.Add("i pissed out the window again, but i locked the bathroom door so she couldnt stop me");
        AddState(new CharTalk(dialogue));
    }
 
    
    public override void OnUpdateState()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            SetState<NPCChase>();
        }
        else if (Input.GetKeyDown(KeyCode.T))
        {
            SetState<NPCRoam>();
        }
        else if (Input.GetKeyDown(KeyCode.U))
        {
            Machine.transform.position = Game.Player.transform.position;
        }

        if (Vector3.Distance(Machine.transform.position, Game.Player.transform.position) < 0.7f)
        {
            PlayerStatus.hit(10, 8, Machine.transform.position);
        }
    }
} 