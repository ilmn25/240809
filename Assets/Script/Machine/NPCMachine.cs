 
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NPCMachine : EntityMachine
{
    public override void OnInitialize()
    {
        State = new NPCState(); 
        AddModule(new NPCMovementModule());
        AddModule(new CharNpcPathFindModule());
        AddModule(new NPCAnimationModule()); 
        AddModule(new SpriteCullModule()); 
        AddModule(new SpriteOrbitModule()); 
    } 
}

public class NPCState : State
{ 
    public override void OnEnterState()
    {
        GUIDialogueSingleton.DialogueAction += DialogueAction; 
        
        AddState(new NPCIdle(), true);
        AddState(new NPCChase());
        AddState(new NPCRoam());
        DialogueData dialogueData = new DialogueData();
        dialogueData.Lines.Add("help");
        dialogueData.Lines.Add("I cant fix my raycast ");
        dialogueData.Lines.Add("im about to kms rahhhhhhh");
        AddState(new CharTalk(dialogueData));
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