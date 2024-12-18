 
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


[System.Serializable]
public class NPCCED : ChunkEntityData
{
    public string npcStatus = "idle";
}

public class NPCMachine : EntityMachine
{ 
    public override void OnInitialize()
    {
        AddState(new NPCState()); 
        AddModule(new NPCMovementModule());
        AddModule(new NPCPathingModule());
        AddModule(new NPCAnimationModule()); 
        AddModule(new SpriteCullModule()); 
        AddModule(new SpriteOrbitModule()); 
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
        GUIDialogue.DialogueAction += DialogueAction; 
        
        string status = ((NPCCED)((EntityMachine)Machine).entityData).npcStatus;
        AddState(new NPCIdle(), status == "idle");
        AddState(new NPCChase(), status == "chase");
        AddState(new NPCRoam(),status == "roam");
        Dialogue dialogue = new Dialogue();
        dialogue.Lines.Add("help");
        dialogue.Lines.Add("I cant fix my raycast ");
        dialogue.Lines.Add("im about to kms rahhhhhhh");
        AddState(new CharTalk(dialogue));
    }

    private void DialogueAction()
    {
        if (Vector3.Distance(Machine.transform.position, Game.Player.transform.position) < 1.4f)
        {
            SetState<CharTalk>();
        }
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
 
    }
} 