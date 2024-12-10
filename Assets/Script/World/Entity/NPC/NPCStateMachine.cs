 
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NPCStateMachine : EntityStateMachine
{
    private NPCMovementModule _npcMovementModule;
    private NPCPathFindAbstract _npcPathFindAbstract;
    private NPCAnimationModule _npcAnimationModule; 
    private SpriteRenderer _sprite;
    protected override void OnAwake()
    {
        GUIDialogueSingleton.DialogueAction += DialogueAction;
        _sprite = transform.Find("sprite").GetComponent<SpriteRenderer>();
        _npcMovementModule = GetComponent<NPCMovementModule>();
        _npcPathFindAbstract = GetComponent<NPCPathFindAbstract>();
        _npcAnimationModule = GetComponent<NPCAnimationModule>();
        
        AddState(new NPCIdle(_npcMovementModule, _npcAnimationModule), true);
        AddState(new NPCChase(_npcMovementModule, _npcPathFindAbstract, _npcAnimationModule, _sprite));
        AddState(new NPCRoam(_npcMovementModule, _npcPathFindAbstract, _npcAnimationModule, _sprite));
        DialogueData dialogueData = new DialogueData();
        dialogueData.Lines.Add("help");
        dialogueData.Lines.Add("I cant fix my raycast ");
        dialogueData.Lines.Add("im about to kms rahhhhhhh");
        AddState(new CharTalk(this, dialogueData));
    }

    private void DialogueAction()
    {
        if (Vector3.Distance(transform.position, Game.Player.transform.position) < 1.4f)
        {
            SetState<CharTalk>();
        }
    }

    public void OnEnable()
    {
        SetState<NPCIdle>();
        _npcMovementModule.SetDirection(Vector3.zero);
    }
    
    protected override void LogicUpdate()
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
            transform.position = Game.Player.transform.position;
        }
 
    }
} 