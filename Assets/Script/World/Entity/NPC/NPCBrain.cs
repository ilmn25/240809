 
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NPCBrain : StateMachine
{
    public override void OnAwake()
    {
        NPCState state = new NPCState();
        state._sprite = transform.Find("sprite").GetComponent<SpriteRenderer>();
        state._npcMovementModule = GetComponent<NPCMovementModule>();
        state._npcPathFindAbstract = GetComponent<NPCPathFindAbstract>();
        state._npcAnimationModule = GetComponent<NPCAnimationModule>();
        State = state;
    } 
}

public class NPCState : State
{
    public NPCMovementModule _npcMovementModule;
    public NPCPathFindAbstract _npcPathFindAbstract;
    public NPCAnimationModule _npcAnimationModule; 
    public SpriteRenderer _sprite;
    public override void OnEnterState()
    {
        GUIDialogueSingleton.DialogueAction += DialogueAction; 
        
        AddState(new NPCIdle(_npcMovementModule, _npcAnimationModule), true);
        AddState(new NPCChase(_npcMovementModule, _npcPathFindAbstract, _npcAnimationModule, _sprite));
        AddState(new NPCRoam(_npcMovementModule, _npcPathFindAbstract, _npcAnimationModule, _sprite));
        DialogueData dialogueData = new DialogueData();
        dialogueData.Lines.Add("help");
        dialogueData.Lines.Add("I cant fix my raycast ");
        dialogueData.Lines.Add("im about to kms rahhhhhhh");
        AddState(new CharTalk(dialogueData));
    }

    private void DialogueAction()
    {
        if (Vector3.Distance(Root.transform.position, Game.Player.transform.position) < 1.4f)
        {
            SetState<CharTalk>();
        }
    }

    public void OnEnable()
    {
        SetState<NPCIdle>();
        _npcMovementModule.SetDirection(Vector3.zero);
    }
    
    public override void StateUpdate()
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
            Root.transform.position = Game.Player.transform.position;
        }
 
    }
} 